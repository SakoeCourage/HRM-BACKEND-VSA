using Carter;
using HRM_BACKEND_VSA.Serivices.Mail_Service;
using HRM_BACKEND_VSA.Shared;
using MediatR;
using static HRM_BACKEND_VSA.Domains.TextMail;

namespace HRM_BACKEND_VSA.Domains
{
    public static class TextMail
    {
        public class IMailRequest : IRequest<Shared.Result>
        {

        }

        internal sealed class Handler : IRequestHandler<IMailRequest, Shared.Result>
        {
            private readonly MailService _mailService;
            public Handler(MailService mailService)
            {
                _mailService = mailService;
            }
            public async Task<Shared.Result> Handle(IMailRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    _mailService.SendMail(new Contracts.EmailDTO
                    {
                        ToEmail = "akorlicourage@gmail.com",
                        ToName = "Akorlicourage",
                        Subject = "Text Email",
                        Body = "This is a test Email"
                    });
                    return Shared.Result.Success();
                }
                catch (Exception ex)
                {
                    return Shared.Result.Failure(Error.BadRequest(ex.Message));
                }

            }
        }
    }
}



public class MapTextMaillEndpoint : ICarterModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/test-mail", async (ISender sender) =>
        {
            var response = await sender.Send(new IMailRequest { });

            if (response.IsSuccess)
            {
                return Results.NoContent();
            }
            if (response.IsFailure)
            {
                return Results.BadRequest(response.Error);
            }
            return Results.BadRequest();
        }).WithTags("Test Mail")
        ;
    }
}
