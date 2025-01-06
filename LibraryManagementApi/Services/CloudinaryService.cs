//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;
//using Microsoft.Extensions.Configuration;
//using Microsoft.AspNetCore.Http;
//using System.Threading.Tasks;

//namespace LibraryManagementApi.Services // Adjust namespace to fit your structure
//{
//    public interface ICloudinaryService
//    {
//        Task<string> UploadImageAsync(IFormFile image);
//    }

//    public class CloudinaryService : ICloudinaryService
//    {
//        private readonly Cloudinary _cloudinary;

//        public CloudinaryService(IConfiguration config)
//        {
//            var cloudName = config["Cloudinary:CloudName"];
//            var apiKey = config["Cloudinary:ApiKey"];
//            var apiSecret = config["Cloudinary:ApiSecret"];
//            var acc = new Account(cloudName, apiKey, apiSecret);
//            _cloudinary = new Cloudinary(acc);
//        }

//        public async Task<string> UploadImageAsync(IFormFile image)
//        {
//            if (image == null || image.Length == 0)
//                throw new ArgumentException("Image file is required.");

//            var uploadParams = new ImageUploadParams()
//            {
//                File = new FileDescription(image.FileName, image.OpenReadStream()),
//                Transformation = new Transformation().Height(500).Width(500).Crop("fill")
//            };

//            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

//            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
//                throw new Exception("Error uploading image to Cloudinary");

//            return uploadResult.SecureUrl.AbsoluteUri;
//        }
//    }
//}
