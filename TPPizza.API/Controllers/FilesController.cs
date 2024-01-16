using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPPizza.API.Models;
using TPPizza.Business;

namespace TPPizza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileService _fileService;

        public FilesController(FileService fileService)
        {
                _fileService = fileService;
        }

        [HttpPost]
        public async void PostAsync([FromBody] FileModel fileModel)
        {
            var data = Convert.FromBase64String(fileModel.Data);

            await _fileService.UploadAsync(fileModel.FileName, data, fileModel.ContentType);
        }

        [HttpGet("{fileName}")]
        public async Task<string> GetAsync(string fileName)
        {
            byte[] bytes = await _fileService.GetAsync(fileName);
            return Convert.ToBase64String(bytes);
        }
    }
}
