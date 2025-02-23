using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Serivices.ImageKit;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio.UpdateOrAddStaffPassport;

namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio
{
    public static class UpdateOrAddStaffPassport
    {
        public class UpdateOrAddStaffPassportRequest : IRequest<Shared.Result<string>>
        {
            public Guid staffId { get; set; }
            public IFormFile passportPicture { get; set; }
        }

        public class Validator : AbstractValidator<UpdateOrAddStaffPassportRequest>
        {
            public Validator()
            {
                RuleFor(c => c.passportPicture)
                    .NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<UpdateOrAddStaffPassportRequest, Shared.Result<string>>
        {
            private readonly IValidator<UpdateOrAddStaffPassportRequest> _validator;
            private readonly HRMDBContext _dbContext;
            private readonly ImageKit _imageKit;
            public Handler(HRMDBContext dbContext, IValidator<UpdateOrAddStaffPassportRequest> validator, ImageKit imageKit)
            {
                _validator = validator;
                _dbContext = dbContext;
                _imageKit = imageKit;
            }
            public async Task<Shared.Result<string>> Handle(UpdateOrAddStaffPassportRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = _validator.Validate(request);
                if (validationResponse.IsValid is false) return Shared.Result.Failure<string>(Error.ValidationError(validationResponse));

                var currentStaff = await _dbContext.Staff.FirstOrDefaultAsync(s => s.Id == request.staffId);
                if (currentStaff is null) return Shared.Result.Failure<string>(Error.CreateNotFoundError("Staff Not Found"));

                try
                {
                    var imageUpladStatus = await _imageKit.HandleNewFormFileUploadAsync(request.passportPicture);
                    if (imageUpladStatus.thumbnailUrl is not null)
                    {
                        currentStaff.passportPicture = imageUpladStatus.thumbnailUrl;
                        await _dbContext.SaveChangesAsync();
                        return Shared.Result.Success(imageUpladStatus.thumbnailUrl);
                    }
                }
                catch (Exception ex)
                {
                    return Shared.Result.Failure<string>(Error.BadRequest(ex.Message));
                }
                return Shared.Result.Failure<string>(Error.BadRequest("Failed Attach Passport Picture"));
            }
        }
    }
}

public class MapUpdateOrAddStaffPassportEnpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/staff/{Id}/update-passportpic", async (ISender sender, Guid Id, [FromForm] IFormFile passportPicture) =>
        {
            var response = await sender.Send(
                new UpdateOrAddStaffPassportRequest
                {
                    staffId = Id,
                    passportPicture = passportPicture
                }
                );

            if (response.IsFailure)
            {
                return Results.UnprocessableEntity(response.Error);
            }
            if (response.IsSuccess)
            {
                return Results.NoContent();
            }

            return Results.BadRequest();

        }).WithTags("Staff-Bio")
              .WithMetadata(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent))
              .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status400BadRequest))
              .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Planning)
              ;
    }
}

