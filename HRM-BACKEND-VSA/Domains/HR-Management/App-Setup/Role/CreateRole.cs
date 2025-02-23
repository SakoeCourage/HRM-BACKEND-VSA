using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Features.Role;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Features.Role
{
    public static class CreateRole
    {

        public class CreateRoleDTO : IRequest<Shared.Result<Guid>>
        {
            public string Name { get; set; }
        }

        public class Validator : AbstractValidator<CreateRoleDTO>
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public Validator(IServiceScopeFactory scopeFactory)
            {

                _scopeFactory = scopeFactory;

                RuleFor(c => c.Name)
                    .NotEmpty()
                    .MustAsync(async (name, cancellation) =>
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetService<HRMDBContext>();
                            var exist = await dbContext.Role.AnyAsync(r => r.name.ToLower() == name.Trim().ToLower());
                            return !exist;
                        }
                    }
                    )
                    .WithMessage("Role Name Already Exist");
                ;
            }
        }

        internal sealed class RequestHanlder : IRequestHandler<CreateRoleDTO, Shared.Result<Guid>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<CreateRoleDTO> _validator;
            public RequestHanlder(HRMDBContext dbContext, IValidator<CreateRoleDTO> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<Shared.Result<Guid>> Handle(CreateRoleDTO request, CancellationToken cancellationtoken)
            {

                if (request == null) return Shared.Result.InvalidRequest<Guid>();

                var validationResponse = await _validator.ValidateAsync(request);

                if (!validationResponse.IsValid)
                {
                    return Shared.Result.Failure<Guid>(new Error(StatusCodes.Status422UnprocessableEntity.ToString(), validationResponse));
                }

                var newRole = new Entities.Role
                {
                    name = request.Name,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };

                _dbContext.Add(newRole);
                await _dbContext.SaveChangesAsync();
                return newRole.Id;

            }
        }
    }
}


public class NewRoleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/role", async (CreateRole.CreateRoleDTO request, ISender sender) =>
        {
            var response = await sender.Send(request);
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.Ok(response.Value);
        }).WithTags("Setup-Role")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Setup)
            ;
    }
}
