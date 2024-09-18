using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElGato_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserDataController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;

        public UserDataController(IJwtService jwtService, IUserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetUserCaloriesIntake()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _userService.GetUserCalories(userId);
                if (!res.error.Success)
                    return BadRequest(res.error.ErrorMessage);

                return Ok(res.model);
            }
            catch (Exception ex) 
            { 
                return StatusCode(500, ex.Message);
            }
        }
    }
}
