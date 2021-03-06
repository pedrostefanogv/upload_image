using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUpload.Data.Data
{
    public class FileService : IFileService
    {
        private static IList<File> _files = new List<File>();

        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IList<File>> GetFiles()
        {
            return await Task.Run(() =>
            {
                return _files;
            });
        }

        public async Task SaveFiles(List<IFormFile> files)
        {
            foreach (var file in files)
            {
                string fileName = GenerateNewFileName(file.FileName);
                string fileFormat = GetFileFormat(file.FileName);
                string filenamefinal = fileName + fileFormat;
                byte[] bytesFile = ConvertFileInByteArray(file);
                string base64 = Convert.ToBase64String(bytesFile);
                Console.WriteLine("base64:# " + base64 + " #");
                byte[] bytesFile2 = Convert.FromBase64String(base64);


                string directory = CreateFilePath(filenamefinal);
                await System.IO.File.WriteAllBytesAsync(directory, bytesFile);

                var url = GetFileUrl(filenamefinal);
                _files.Add(new File(
                    url,
                    fileFormat));
            }
        }

        private string GetFileFormat(string fullFileName)
        {
            var format = fullFileName.Split(".").Last();
            return "." + format;
        }

        private string GenerateNewFileName(string fileName)
        {

            var newFileName = (Guid.NewGuid().ToString()).ToLower();
            newFileName = newFileName.Replace("-", "");

            return newFileName;
        }

        private string CreateFilePath(string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), _configuration["Directories:Files"], fileName);
        }

        private string GetFileUrl(string fileName)
        {
            var baseUrl = _configuration["Directories:BaseUrl"];

            var fileUrl = _configuration["Directories:Files"]
                .Replace("wwwroot", "")
                .Replace("\\", "");

            return (baseUrl + "/" + fileUrl + "/" + fileName);
        }

        private byte[] ConvertFileInByteArray(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}