using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Department.GetDepartmentById;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Department
{
    public static class GetDepartmentById
    {
        public class GetDepartmentByIdRequest : IRequest<Shared.Result<Entities.Department>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<GetDepartmentByIdRequest, Shared.Result<Entities.Department>>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Shared.Result<Entities.Department>> Handle(GetDepartmentByIdRequest request, CancellationToken cancellationToken)
            {
                var response = await _dbContext.Department
                    .Include(x => x.headOfDepartment)
                    .Include(x => x.depHeadOfDepartment)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);
                if (response is null)
                {
                    return Shared.Result.Failure<Entities.Department>(Error.NotFound);
                }
                return Shared.Result.Success(response);
            }
        }
    }
}

public class GetDepartmentByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/department/{Id}",
        async (ISender sender, Guid id) =>
        {

            var response = await sender.Send(new GetDepartmentByIdRequest
            {
                Id = id
            });

            if (response.IsFailure)
            {
                return Results.BadRequest(response?.Error);
            }

            if (response.IsSuccess)
            {
                return Results.Ok(response.Value);
            }

            return Results.BadRequest(response?.Error);
        })
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_BACKEND_VSA.Entities.Department), StatusCodes.Status200OK))
            .WithTags("Setup-Department");
    }
}

