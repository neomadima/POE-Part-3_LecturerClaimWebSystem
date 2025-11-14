namespace LecturerClaimsSystem.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
        private readonly string[] ALLOWED_EXTENSIONS = { ".pdf", ".docx", ".xlsx" };

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MAX_FILE_SIZE)
                throw new InvalidOperationException("File size exceeds the 10MB limit.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!ALLOWED_EXTENSIONS.Contains(extension))
                throw new InvalidOperationException("Invalid file type. Only PDF, DOCX, and XLSX files are allowed.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }

        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !filePath.StartsWith("/uploads/"))
                return false;

            var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                return true;
            }
            return false;
        }
    }
}