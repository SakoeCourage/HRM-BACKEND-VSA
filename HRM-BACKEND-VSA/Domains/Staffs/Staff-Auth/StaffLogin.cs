using AutoMapper;
using Carter;
using FluentValidation;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Extensions;
using HRM_BACKEND_VSA.Providers;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Contracts.StaffContracts;
using static HRM_BACKEND_VSA.Domains.Staffs.Staff_Auth.StaffLogin;

namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Auth
{
    public static class StaffLogin
    {
        public class StaffLoginRequest : IRequest<Shared.Result<StaffLoginResponse>>
        {
            public string staffIdentificationNumber { get; set; }
            public string password { get; set; }
        }

        public class Validator : AbstractValidator<StaffLoginRequest>
        {
            public Validator()
            {
                RuleFor(c => c.staffIdentificationNumber).NotEmpty();
                RuleFor(c => c.password).NotEmpty();
            }
        }


        internal sealed class Handler : IRequestHandler<StaffLoginRequest, Shared.Result<StaffLoginResponse>>
        {
            private readonly HRMDBContext _dbContext;
            private readonly IMapper _mapper;
            private readonly JWTProvider _jwtProvider;
            private readonly IValidator<StaffLoginRequest> _validator;
            public Handler(HRMDBContext dbContext, IMapper mapper, JWTProvider jwtProvider, IValidator<StaffLoginRequest> validator)
            {
                _dbContext = dbContext;
                _mapper = mapper;
                _jwtProvider = jwtProvider;
                _validator = validator;
            }
            public async Task<Result<StaffLoginResponse>> Handle(StaffLoginRequest request, CancellationToken cancellationToken)
            {
                var validationResponse = _validator.Validate(request);

                if (validationResponse.IsValid is false)
                {
                    return Shared.Result.Failure<StaffLoginResponse>(Error.ValidationError(validationResponse));
                }

                var staffData = await _dbContext.Staff
                        .FirstOrDefaultAsync(s => s.staffIdentificationNumber == request.staffIdentificationNumber);
                
                if (staffData == null)
                {
                    return Shared.Result.Failure<StaffLoginResponse>(Error.CreateNotFoundError("Invalid Staff Id Or Password"));
                }
                var isPasswordMatch = BCrypt.Net.BCrypt.Verify(request.password, staffData.password);

                if (isPasswordMatch is false)
                {
                    return Shared.Result.Failure<StaffLoginResponse>(Error.CreateNotFoundError("Invalid Staff Id Or Password"));
                }
                
                var staffPostingData = await _dbContext.StaffPosting
                    .Include(entry=>entry.directorate)
                    .Include(entry=>entry.unit)
                    .Include(entry=>entry.department)
                    .FirstOrDefaultAsync(pt=>pt.staffId == staffData.Id);

                if (staffPostingData is null)
                {
                    return Shared.Result.Failure<StaffLoginResponse>(Error.BadRequest("Staff Posting Data Not Found. Please Contact Administrator"));
                }

                var response = new StaffLoginResponse
                {
                    Id = staffData.Id,
                    staffIdentificationNumber = staffData.staffIdentificationNumber,
                    firstName = staffData.firstName,
                    lastName = staffData.lastName,
                    otherNames = staffData.otherNames,
                    gender = staffData.gender,
                    email = staffData.email,
                    passportPicture = staffData.passportPicture,
                    directorate = new SetupContract.DirectorateListResponseDto
                    {
                        Id = staffPostingData.unit.directorate.Id,
                        createdAt = staffPostingData.unit.directorate.createdAt,
                        updatedAt = staffPostingData.unit.directorate.updatedAt,
                        directorateName = staffPostingData.unit.directorate.directorateName,
                    },
                    unit = new StaffContracts.StaffUnitResponsePartial
                    {
                        Id = staffPostingData.unit.Id,
                        createdAt = staffPostingData.unit.createdAt,
                        updatedAt = staffPostingData.unit.updatedAt,
                        unitName = staffPostingData.unit.unitName,
                    },
                    department = new SetupContract.DepartmentListResponseDto
                    {
                        Id = staffPostingData.department.Id,
                        createdAt = staffPostingData.department.createdAt,
                        updatedAt = staffPostingData.department.updatedAt,
                        departmentName = staffPostingData.department.departmentName,
                    }

                };

                response.accessToken = _jwtProvider.GenerateAccessToken(staffData.Id, AuthorizationDecisionType.Staff);

                return Shared.Result.Success(response);

            }
        }
    }
}

public class MapStaffLoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/staff/login", async (ISender sender, StaffLoginRequest request) =>
        {
            var result = await sender.Send(request);

            if (result.IsFailure)
            {
                return Results.BadRequest(result?.Error);
            }
            if (result.IsSuccess)
            {
                return Results.Ok(result?.Value);
            }

            return Results.BadRequest();
        })
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(StaffLoginResponse), StatusCodes.Status200OK))
        .WithMetadata(new ProducesResponseTypeAttribute(typeof(Error), StatusCodes.Status422UnprocessableEntity))
        .WithTags("Authentication-Staff")
        .WithGroupName(SwaggerDoc.SwaggerEndpointDefintions.HRAuthService)
          ;
    }
}
