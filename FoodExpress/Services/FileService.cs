using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FoodExpress.Services
{
    public interface IFileService
    {
        Task<string?> SaveFileAsync(IFormFile? file, string subFolder);
        void DeleteFile(string? relativeFilePath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveFileAsync(IFormFile? file, string subFolder)
        {
            if (file == null || file.Length == 0) return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Invalid image file type. Allowed types: jpg, jpeg, png, webp, gif.");
            }

            var folderPath = Path.Combine(_environment.WebRootPath, "images", subFolder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/images/{subFolder}/{uniqueFileName}";
        }

        public void DeleteFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath)) return;

            // Remove leading slash if any
            var cleanPath = relativeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, cleanPath);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    // Ignore deletion failures if file is in use or missing
                }
            }
        }
    }
}
