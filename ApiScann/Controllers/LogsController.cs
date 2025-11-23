using ApiScann.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiScann.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ScanLog>>> GetAll()
        {
            return await _context.ScanLog.ToListAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<List<ScanLog>>> GetAllbyID(Guid id)
        {
            var log = await _context.ScanLog.FindAsync(id);

            if(log == null)
            {
                return NotFound(new { message = "Log no encontrado", id });
            }
            return Ok(log);
        }
    }
}
