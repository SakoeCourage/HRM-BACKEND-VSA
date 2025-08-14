using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.CreateUser;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.UpdateUser;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User
{
    public class UpdateUser
    {
        public class UpdateUserRequest : IRequest<Shared.Result<Guid>>
        {
            public Guid id { get; set; }
            public Guid roleId { get; set; }
            public Guid staffId { get; set; }
            public Guid unitId { get; set; }
            public Guid departmentId { get; set; }
            public string email { get; set; }

        }
    }

    public class Validator : AbstractValidator<UpdateUserRequest>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public Validator(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public Validator()
        {
            RuleFor(c => c.email).NotEmpty().EmailAddress()
                .MustAsync(async (email, cancellationToken) =>
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                        var exist = await dbContext.User.AnyAsync(u => u.email == email, cancellationToken);
                        return !exist;
                    }
                }).WithMessage("User Email Is Already Taken");
            RuleFor(c => c.roleId).NotEmpty();
            RuleFor(c => c.unitId).NotEmpty();
            RuleFor(c => c.staffId).NotEmpty();

        }
        internal sealed class RequestHandler : IRequestHandler<UpdateUserRequest, Shared.Result<Guid>>
        {
            private readonly IValidator<UpdateUserRequest> _validator;
            private readonly HRMDBContext _dbContext;

            public RequestHandler(IValidator<UpdateUserRequest> validator, HRMDBContext dbContext)
            {
                _dbContext = dbContext;
                _validator = validator;
            }
            public async Task<Result<Guid>> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = await _validator.ValidateAsync(request, cancellationToken);
                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResponse));
                }

                var userStaffData = await _dbContext.Staff.FirstOrDefaultAsync(s => s.Id == request.staffId);

                if (userStaffData is null)
                {
                    return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("User Staff Data Not Found"));
                }

                var existingUser = await _dbContext.User.FirstOrDefaultAsync(s => s.Id == request.id);

                if (existingUser is null)
                {
                    return Shared.Result.Failure<Guid>(Error.CreateNotFoundError("User Data Not Found"));
                }

                using (var transaction = _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        existingUser.roleId = request.roleId;
                        existingUser.staffId = request.staffId;
                        existingUser.updatedAt = DateTime.UtcNow;
                        existingUser.email = request.email;

                        var existingUserRole = await _dbContext.UserHasRole.FindAsync(request.roleId);
                        if (existingUserRole is not null)
                        {
                            existingUserRole.roleId = request.roleId;
                        }
                        await _dbContext.SaveChangesAsync();
                        await transaction.Result.CommitAsync();
                        return Shared.Result.Success(existingUser.Id);
                    }
                    catch (Exception ex)
                    {
                        await transaction.Result.RollbackAsync();
                        return Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));
                    }
                }


            }
        }

    }

}
public class MapUpdateUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/user/{id}", async (ISender sender, Guid id, CreateUserRequest request) =>
        {
            var response = await sender.Send(new UpdateUserRequest
            {
                id = id,
                roleId = request.roleId,
                staffId = request.staffId,
                email = request.email,
                unitId = request.unitId
            });

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }
            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }

            return Results.BadRequest();
        }).WithTags("Manage-User")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.UserManagement)
            ;
    }
}
