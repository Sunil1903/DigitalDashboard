using DigitalDashboard.DAL.Models;
using DigitalDashboard.JobManager.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalDashboard.JobManager.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogRepository logRepository;
        public LogController(ILogRepository logRepository) =>
            this.logRepository = logRepository;


        [HttpGet, Route("digitaldashboard/Log/GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAll()
        {
            var result = await logRepository.GetAllLog();
            return result == null ? NoContent() : Ok(result);
        }

        [HttpPost, Route("digitaldashboard/Log/Logging")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Log(Log newLog)
        {
            await logRepository.Logging(newLog);

            return Ok(newLog);
        }
    }
}
