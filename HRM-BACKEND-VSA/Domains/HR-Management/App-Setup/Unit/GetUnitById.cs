﻿using Carter;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit.GetUnitById;

namespace HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Unit
{
    public static class GetUnitById
    {
        public class GetUnitByIdRequest : IRequest<Shared.Result<Entities.Unit>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<GetUnitByIdRequest, Shared.Result<Entities.Unit>>
        {
            private readonly HRMDBContext _dbContext;
            public Handler(HRMDBContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Shared.Result<Entities.Unit>> Handle(GetUnitByIdRequest request, CancellationToken cancellationToken)
            {
                var role = await _dbContext.Unit.Include(x => x.department)
                    .Include(x => x.directorate)
                    .Include(x => x.unitHead)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (role is null)
                {
                    return Shared.Result.Failure<Entities.Unit>(Error.NotFound);
                }
                return Shared.Result.Success(role);
            }
        }

    }
}

public class GetUnitByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("api/unit/{Id}",
        async (ISender sender, Guid id) =>
        {

            var response = await sender.Send(new GetUnitByIdRequest
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
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(HRM_BACKEND_VSA.Entities.Unit), StatusCodes.Status200OK))
            .WithTags("Setup-Unit")
            ;
    }
}
