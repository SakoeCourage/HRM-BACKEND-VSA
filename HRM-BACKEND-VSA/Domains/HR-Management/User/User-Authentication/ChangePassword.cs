using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication.ChangePassword;

namespace HRM_BACKEND_VSA.Domains.HR_Management.User.User_Authentication
{
    public class ChangePassword
    {
        public class ChangePasswordRequest : IRequest<Shared.Result<string>>
        {
            public string currentPassword { get; set; }
            public string newPassword { get; set; }
        }

        public class Validator : AbstractValidator<ChangePasswordRequest>
        {
            public Validator()
            {
                RuleFor(c => c.currentPassword).NotEmpty();
                RuleFor(c => c.newPassword).NotEmpty().MinimumLength(5);
            }
        }

        internal sealed class Handler : IRequestHandler<ChangePasswordRequest, Shared.Result<string>>
        {
            private readonly Authprovider _authProvider;
            private readonly HRMDBContext _dbContext;
            private readonly IValidator<ChangePasswordRequest> _validator;

            public Handler(Authprovider authProvider, HRMDBContext dbContext,
                IValidator<ChangePasswordRequest> validator)
            {
                _authProvider = authProvider;
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<Result<string>> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (validationResult.IsValid is false)
                {
                    return Shared.Result.Failure<string>(Error.ValidationError(validationResult));
                }

                var authUser = await _authProvider.GetAuthUser();

                if (authUser is null)
                {
                    return Shared.Result.Failure<string>("User Not Found");
                }

                var passwordMatach = BCrypt.Net.BCrypt.Verify(request.currentPassword, authUser.password);
                if (passwordMatach is false)
                {
                    return Shared.Result.Failure<string>(Error.BadRequest("Failed To Verify Current Password"));
                }

                var affectedRow = await _dbContext.User
                    .Where(x => x.Id == authUser.Id)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(c => c.password, BCrypt.Net.BCrypt.HashPassword(request.newPassword))
                            .SetProperty(c => c.updatedAt, DateTime.UtcNow)
                            .SetProperty(c => c.hasResetPassword, true)
                    );

                if (affectedRow == 0)
                {
                    return Shared.Result.Failure<string>("Failed To Update Password. Please Try Again");
                }

                return Shared.Result.Success("Password Updated Successfully");
            }
        }
    }
}


public class MapChangePasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/user/password-reset",
                [Authorize(Policy = AuthorizationDecisionType.HRMUser)]
                async (ISender sender, ChangePasswordRequest request) =>
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