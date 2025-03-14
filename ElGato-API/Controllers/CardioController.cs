using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.VMO.Cardio;
using ElGato_API.VMO.ErrorResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ElGato_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CardioController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly ICardioService _cardioService;
        public CardioController(IJwtService jwtService, ICardioService cardioService)
        {
            _jwtService = jwtService;
            _cardioService = cardioService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ChallengeVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetActivChallenges()
        {
            try
            {
                var res = await _cardioService.GetActiveChallenges();
                if (!res.error.Success)
                {
                    return res.error.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res.error),
                        _ => BadRequest(res.error)
                    };
                }

                return Ok(res.data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured: {ex.Message}", Success = false });
            }
        }
    }
}
