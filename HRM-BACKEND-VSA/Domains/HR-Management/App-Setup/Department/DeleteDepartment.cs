using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Department.DeleteDepartment;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Department
{
    public static class DeleteDepartment
    {
        public class DeleteDepartmentRequest : IRequest<Shared.Result>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<DeleteDepartmentRequest, Shared.Result>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Shared.Result> Handle(DeleteDepartmentRequest request, CancellationToken cancellationToken)
            {
                var affectedRows = await _dbContext
                    .Department
                    .Where(s => s.Id == request.Id)
                    .ExecuteDeleteAsync();

                if (affectedRows == 0) return Shared.Result.Failure(Error.CreateNotFoundError("Department Not Found"));
                return Shared.Result.Success();
            }
        }
    }
}

public class DeleteDepartmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/department/{Id}", async (ISender sender, Guid Id) =>
        {
            var response = await sender.Send(new DeleteDepartmentRequest { Id = Id });

            if (response.IsFailure)
            {
                return Results.NotFound(response.Error);
            }

            return Results.NoContent();

        }).WithTags("Setup-Department")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
          ;
    }
}
