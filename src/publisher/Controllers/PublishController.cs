using Microsoft.AspNetCore.Mvc;
using publisher.Services;

namespace publisher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublishController : ControllerBase
    {
        private readonly Services.SqsService _sqsService;
        private readonly ILogger<PublishController> _logger;

        public PublishController(SqsService sqsService, ILogger<PublishController> logger)
        {
            _sqsService = sqsService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PublishMessage([FromBody] string message)
        {
            try
            {
                await _sqsService.SendMessageAsync(message);
                _logger.LogInformation("Message published to SQS: {Message}", message);
                return Ok("Mensagem publicada com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to SQS");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}