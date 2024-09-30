using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.VM.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElGato_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserRequestController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly IUserRequestService _requestService;
        public UserRequestController(IJwtService jwtService, IUserRequestService requestService)
        {
            _jwtService = jwtService;
            _requestService = requestService;
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> ReportIngredientRequest([FromBody] IngredientReportRequestVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _requestService.RequestReportIngredient(userId, model);
                if (!res.Success)
                    return StatusCode(400, $"failed, {res.ErrorMessage}");

                return Ok();
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Internal server error. {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> AddIngredientRequest([FromBody] AddProductRequestVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _requestService.RequestAddIngredient(userId, model);
                if (!res.Success)
                    return StatusCode(400, $"failed, {res.ErrorMessage}");

                return Ok();
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Internal server error. {ex.Message}");
            }
        }

    }
}
