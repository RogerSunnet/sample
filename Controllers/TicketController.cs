using AWS.PM.Entity;
using AWS.PM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewTicketApi.Utils;

namespace NewTicketApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TicketController : ControllerBase
    {       
        private readonly ILogger<TicketController> _logger;

        public TicketController(ILogger<TicketController> logger)
        {
            _logger = logger;
        }

        //[MyAuthorize]
        [HttpGet(Name = "Ticket/List")]
        public IEnumerable<TicketEntity> List([FromServices] ITicketService ticketService)
        {
            return ticketService.Query<TicketEntity>(o => !string.IsNullOrWhiteSpace(o.Title)).OrderByDescending(o => o.CreatedOn).ToList();
        }

        //[MyAuthorize]
        [HttpGet(Name = "Ticket/Get")]
        public TicketEntity Get([FromServices] ITicketService ticketService, int Id)
        {
            return ticketService.Find<TicketEntity>(Id);
        }

        //[MyAuthorize]
        [HttpPut(Name = "Ticket/Put")]
        public void Put([FromServices] ITicketService ticketService, [FromBody] TicketEntity ticket)
        {
            ticketService.Update<TicketEntity>(ticket);
        }

        //[MyAuthorize]
        [HttpPost(Name = "Ticket/Post")]
        public TicketEntity Post([FromServices] ITicketService ticketService, [FromBody] TicketEntity ticket)
        {
            return ticketService.Insert<TicketEntity>(ticket);
        }

        //[MyAuthorize]
        [HttpDelete(Name = "Ticket/Delete")]
        public void Delete([FromServices] ITicketService ticketService, int Id)
        {
            ticketService.Delete<TicketEntity>(Id);
        }
    }
}