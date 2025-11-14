namespace LecturerClaimsSystem.Services
{
    public interface IFileService
    {
        Task<string?> SaveFileAsync(IFormFile file);
        bool DeleteFile(string filePath);
    }
}