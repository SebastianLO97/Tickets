using Microsoft.AspNetCore.Mvc;
using Tickets.Services;

namespace Tickets.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ILogger<TicketsController> _logger;
        private readonly ITicketService _ticketService;

        public TicketsController(ILogger<TicketsController> logger, ITicketService ticketService)
        {
            _logger = logger;
            _ticketService = ticketService;
        }

        [HttpGet]
        public ActionResult Get()
        {
            _ticketService.InsertTickets();
            return Ok();
        }
    }
}