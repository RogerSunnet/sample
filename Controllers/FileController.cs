using AWS.PM.Entity;
using AWS.PM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewTicketApi.Utils;

namespace NewTicketApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {

        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        //[MyAuthorize]
        [HttpGet(Name = "File")]
        public IEnumerable<FileEntity> Get([FromServices] IFileService fileService, int TickeId)
        {
            return fileService.Query<FileEntity>(o => o.IsDelete == false && o.TicketID == TickeId).OrderBy(o => o.CreatedOn).ToList();
        }

        //[MyAuthorize]
        [HttpPost(Name = "File")]
        public FileEntity Post([FromServices] IFileService fileService, [FromBody] FileEntity file)
        {
            return fileService.Insert<FileEntity>(file);
        }
    }
}