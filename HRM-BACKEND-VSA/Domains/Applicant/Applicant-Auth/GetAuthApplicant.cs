using Carter;
using HRM_BACKEND_VSA.Entities.Applicant;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static HRM_BACKEND_VSA.Features.Applicant.GetAuthApplicant;

namespace HRM_BACKEND_VSA.Features.Applicant
{
    public static class GetAuthApplicant
    {
        public class GetAuthApplicantRequest : IRequest<Result<Entities.Applicant.Applicant>>
        {

        }

        internal sealed class Handler : IRequestHandler<GetAuthApplicantRequest, Result<Entities.Applicant.Applicant>>
        {
            private readonly Authprovider _authProvider;
            public Handler(Authprovider authProvider)
            {
                _authProvider = authProvider;
            }
            public async Task<Result<Entities.Applicant.Applicant?>> Handle(GetAuthApplicantRequest request, CancellationToken cancellationToken)
            {
                var applicant = await _authProvider.GetAuthApplicant();

                if (applicant is null)
                {
                    return Shared.Result.Failure<Entities.Applicant.Applicant>(Error.CreateNotFoundError("Applicant Not Found"));
                }
                return Shared.Result.Success(applicant);

            }
        }
    }
}


public class MapAuthApplicantEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/applicant/auth-applicant",
         [Authorize(Policy = AuthorizationDecisionType.Applicant)]
        async (ISender sender) =>
        {
            var result = await sender.Send(new GetAuthApplicantRequest { });

            if (result.IsFailure)
            {
                return Results.NotFound(result?.Error);
            }
            if (result.IsSuccess)
            {
                return Results.Ok(result?.Value);
            }

            return Results.BadRequest();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Applicant), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("Authentication Applicant")
          ;
    }
}
