using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElGato_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MealController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly IMealService _mealService;
        public MealController(IJwtService jwtService, IMealService mealService)
        {
            _mealService = mealService;
            _jwtService = jwtService;
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetStarters()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();



                return Ok();
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
