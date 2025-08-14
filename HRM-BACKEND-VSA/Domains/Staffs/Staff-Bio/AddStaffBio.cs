using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using HRM_BACKEND_VSA.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio.AddStaffBio;

namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio
{
    public static class AddStaffBio
    {
        public class AddStaffBioRequest : IRequest<Shared.Result<Guid>>
        {
            public string? title { get; set; } = string.Empty;
            public string firstName { get; set; }
            public string lastName { get; set; }

            public string? otherNames { get; set; }
            public Guid specialityId { get; set; }
            public DateOnly dateOfBirth { get; set; }
            public string phone { get; set; }
            public string gender { get; set; }
            public string? SNNITNumber { get; set; } = null;
            public string? ECOWASCardNumber { get; set; } = null;
            public string GPSAddress { get; set; } = string.Empty;
            public string staffIdentificationNumber { get; set; } = string.Empty;
            public string email { get; set; }
            public string disability { get; set; }
        }

        public class Validator : AbstractValidator<AddStaffBioRequest>
        {
            protected readonly IServiceScopeFactory _serviceScopeFactory;
            public Validator(IServiceScopeFactory scopeServiceFactory)
            {
                _serviceScopeFactory = scopeServiceFactory;

                RuleFor(c => c.specialityId).NotEmpty();
                RuleFor(c => c.firstName).NotEmpty();
                RuleFor(c => c.lastName).NotEmpty();
                RuleFor(c => c.phone).NotEmpty();
                RuleFor(c => c.gender).NotEmpty();
                RuleFor(c => c.dateOfBirth).NotEmpty();
                RuleFor(c => c.phone).NotEmpty()
                    .MustAsync(async (model, data, cancelationToken) =>
                    {
                        if (string.IsNullOrWhiteSpace(data))
                        {
                            return true;
                        }

                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            bool exist = await dbContext
                                .Staff
                                .AnyAsync(e => e.phone.ToLower() == data.Trim().ToLower());
                            return !exist;
                        }
                    })
                    .WithMessage("Phone Number Already Exist");
                ;
                RuleFor(c => c.SNNITNumber).NotEmpty()
                    .MustAsync(async (model, data, cancelationToken) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            bool exist = await dbContext
                                .Staff
                                .AnyAsync(e => e.SNNITNumber.ToLower() == data.Trim().ToLower());
                            return !exist;
                        }
                    })
                    .WithMessage("SNNIT Number Already Exist");
                ;
                RuleFor(c => c.ECOWASCardNumber)
                    .MustAsync(async (model, data, cancelationToken) =>
                    {
                        if (string.IsNullOrWhiteSpace(data))
                        {
                            return true;
                        }

                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            bool exist = await dbContext
                                .Staff
                                .AnyAsync(e => e.ECOWASCardNumber.ToLower() == data.Trim().ToLower());
                            return !exist;
                        }
                    })
                    .WithMessage("ECOWAS Card Number Already Exist");
                RuleFor(c => c.email)
                    .NotEmpty()
                    .EmailAddress()
                    .MustAsync(async (model, email, cancelationToken) =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<HRMDBContext>();
                            bool exist = await dbContext
                                .Staff
                                .AnyAsync(e => e.email.ToLower() == email.Trim().ToLower());
                            return !exist;
                        }
                    })
                    .WithMessage("Email Already Exist")
                    ;
            }
        }

        internal sealed class Handler : IRequestHandler<AddStaffBioRequest, Shared.Result<Guid>>
        {
            private readonly IValidator<AddStaffBioRequest> _validator;
            private readonly HRMDBContext _dbContext;
            private readonly Authprovider _authProvider;
            private readonly IMapper _mapper;

            public Handler(HRMDBContext dbContext, IValidator<AddStaffBioRequest> validator, Authprovider authProvider,
                IMapper mapper)
            {
                _validator = validator;
                _dbContext = dbContext;
                _authProvider = authProvider;
                _mapper = mapper;
            }

            public async Task<Result<Guid>> Handle(AddStaffBioRequest request, CancellationToken cancellationToken)
            {
                if (request == null)
                    return Shared.Result.Failure<Guid>(new Error(code: "Invalid Request",
                        message: "Invalid Request body"));

                var validationResult = await _validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Shared.Result.Failure<Guid>(Error.ValidationError(validationResult));
                }

                using (var transactions = await _dbContext.Database.BeginTransactionAsync())
                {
                    Console.WriteLine($"Ecowas card number as {request.ECOWASCardNumber}");
                    try
                    {
                        var rndIdNumber = Stringutilities.GeneraterandomStaffNumbers();
                        var newStaffEntry = new Entities.Staff.Staff
                        {
                            title = request?.title ?? string.Empty,
                            specialityId = request?.specialityId,
                            firstName = request.firstName,
                            lastName = request.lastName,
                            email = request.email,
                            phone = request.phone,
                            gender = request.gender,
                            otherNames = string.IsNullOrWhiteSpace(request?.otherNames)
                                ? null
                                : request.otherNames,
                            SNNITNumber = request?.SNNITNumber ?? null,
                            ECOWASCardNumber = string.IsNullOrWhiteSpace(request?.ECOWASCardNumber)
                                ? null
                                : request.ECOWASCardNumber,
                            disability = request.disability,
                            dateOfBirth = request?.dateOfBirth,
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow,
                            GPSAddress = request?.GPSAddress ?? string.Empty,
                            isApproved = true,
                            status = StaffContracts.StaffStatusTypes.active,
                            staffIdentificationNumber = string.IsNullOrWhiteSpace(request?.staffIdentificationNumber)
                                ? null
                                : request.staffIdentificationNumber,
                            password = BCrypt.Net.BCrypt.HashPassword($"{request.firstName}{request.lastName}")
                        };
                        _dbContext.Add(newStaffEntry);
                        // var staffBioDataRequest = new StaffBioDataRequest(_dbContext, _authProvider, _mapper);
                        // await staffBioDataRequest.NewStaffRequest(newStaffEntry.Id);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await transactions.CommitAsync(cancellationToken);
                        return Shared.Result.Success(newStaffEntry.Id);
                    }
                    catch (Exception ex)
                    {
                        await transactions.RollbackAsync(cancellationToken);
                        return Shared.Result.Failure<Guid>(Error.BadRequest(ex.Message));
                    }
                }
            }
        }
    }
}

public class MapAddStaffioEndpint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff",
                async (AddStaffBioRequest request, ISender sender) =>
                {
                    var response = await sender.Send(request);
                    if (response.IsFailure)
                    {
                        return Results.UnprocessableEntity(response.Error);
                    }

                    return Results.Ok(response.Value);
                }
            ).WithTags("Staff-Bio")
            .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.Planning)
            ;
    }
}