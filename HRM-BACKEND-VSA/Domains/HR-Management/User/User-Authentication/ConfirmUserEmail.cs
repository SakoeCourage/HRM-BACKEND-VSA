using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication.ConfirmUserEmail;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public static class ConfirmUserEmail
    {
        public class ConfirmUserEmailRequest : IRequest<Shared.Result<string>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<ConfirmUserEmailRequest, Shared.Result<string>>
        {
            private readonly HRMDBContext _dbContext;

            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;

            }
            public async Task<Result<string>> Handle(ConfirmUserEmailRequest request, CancellationToken cancellationToken)
            {
                var affetedRow = await _dbContext.User.Where(u => u.Id == request.Id && u.emailVerifiedAt != null)
                    .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        c => c.emailVerifiedAt, DateTime.UtcNow)
                    .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                    );

                if (affetedRow == 0) { return Shared.Result.Failure<string>("Failed To Confirm Email"); }

                return Shared.Result.Success("Your Email Has Been Verified");

            }
        }
    }
}
public class MapConfirmUserEmailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/user/verify-email",
        async (ISender sender, ConfirmUserEmailRequest request) =>
            {
                var response = await sender.Send(request);

                if (response.IsSuccess)
                {
                    return Results.Ok(response.Value);
                }
                if (response.IsFailure)
                {
                    return Results.UnprocessableEntity(response.Error);
                }

                return Results.BadRequest();
            })
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("Authentication-HRM-User")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.HRAuthService)
            ;
    }
}
