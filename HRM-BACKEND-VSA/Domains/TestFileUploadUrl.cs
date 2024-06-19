using Carter;
using HRM_BACKEND_VSA.Serivices.ImageKit;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRM_BACKEND_VSA.Domains
{
    public static class TestFileUploadUrl
    {

        public class RequestData : IRequest<Shared.Result>
        {

            public IFormFile image { get; set; }
        }

        internal sealed class Handler : IRequestHandler<RequestData, Shared.Result>
        {

            private readonly ImageKit _imageKit;
            public Handler(ImageKit imageKit)
            {
                _imageKit = imageKit;
            }
            public async Task<Shared.Result> Handle(RequestData request, CancellationToken cancellationToken)
            {
                var response = await _imageKit.HandleNewFormFileUploadAsync(request.image);

                return null;
            }
        }
    }


    public class MapUploadFileUrlEndpoint : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/file-upload", async (ISender sender, [FromForm] IFormFile image) =>
            {
                await sender.Send(new TestFileUploadUrl.RequestData { image = image });
            }).WithTags("File Upload")
            .DisableAntiforgery();
            ;
        }
    }
}
