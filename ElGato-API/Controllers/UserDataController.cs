using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.User;
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
        [ProducesResponseType(typeof(UserCalorieIntake), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserCaloriesIntake()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _userService.GetUserCalories(userId);
                if (!res.error.Success)
                {
                    return res.error.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res.error),
                        ErrorCodes.Internal => StatusCode(500, res.error),
                        _ => BadRequest(res.error)
                    };
                }                   

                return Ok(res.model);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured {ex.Message}", Success = false });
            }
        }
    }
}
