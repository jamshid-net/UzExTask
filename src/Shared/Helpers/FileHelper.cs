using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace ProjectTemplate.Shared.Helpers;
public static class FileHelper
{
    public static async Task WriteFileAsync(IFormFile file, string filePath, CancellationToken ct = default)
    {
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var fileStream = new FileStream(filePath,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);
        await file.CopyToAsync(fileStream, ct);
    }

    public static async Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

    public static async Task<(byte[] bytes, string cotentType, string fileName)> GetFileAsync(string filePath, CancellationToken ct = default)
    {
        
        var bytes = await File.ReadAllBytesAsync(filePath, ct);

        // Determine Content-Type
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var  contentType))
        {
            contentType = "application/octet-stream"; // Default binary type
        }

        // Extract file name
        string fileName = Path.GetFileName(filePath);
        return (bytes, contentType, fileName);
        

    }
}
