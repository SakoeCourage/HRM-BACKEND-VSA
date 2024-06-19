using Imagekit.Sdk;

namespace HRM_BACKEND_VSA.Serivices.ImageKit
{
    public class ImageKit
    {
        private readonly IConfigurationSection _imageKitConfigSection;
        private ImagekitClient imagekit;
        public ImageKit(IConfiguration configuration)
        {
            _imageKitConfigSection = configuration.GetSection("ImageKitSettings");
            imagekit = new ImagekitClient(
                publicKey: _imageKitConfigSection["publicKey"],
                privateKey: _imageKitConfigSection["privateKey"],
                urlEndPoint: _imageKitConfigSection["UrlEndpoint"]);
        }

        public async Task<byte[]> ConvertIFormFileToBytesAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task<Result> HandleNewFormFileUploadAsync(IFormFile file)
        {
            if (file is null) return null;
            try
            {
                var fileName = file.FileName;
                var fileExtension = Path.GetExtension(fileName);
                var fileInBytes = await this.ConvertIFormFileToBytesAsync(file);
                var response = await imagekit.UploadAsync(
                    new FileCreateRequest
                    {
                        file = fileInBytes,
                        fileName = $"{Guid.NewGuid().ToString()}{fileExtension}",
                        folder = "/KBTH"
                    }
                );
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void BulkDelete(List<string> thumbnailUrl)
        {
            imagekit.BulkDeleteFiles(thumbnailUrl);

        }


    }
}

