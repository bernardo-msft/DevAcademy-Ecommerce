

using Microsoft.AspNetCore.Http;

namespace Ecommerce.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string fileName);
    Task DeleteFileAsync(string fileName);
}