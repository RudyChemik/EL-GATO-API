using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElGato_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DietController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly IDietService _dietService;

        public DietController(IJwtService jwtService, IDietService dietService) 
        { 
            _jwtService = jwtService;
            _dietService = dietService;
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> AddNewMeal(string? mealName, DateTime date)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }
                try
                {
                    var res = await _dietService.AddNewMeal(userId, mealName, date);
                    if (!res.Success)
                    {
                        return StatusCode(400, res.ErrorMessage);
                    }

                    return Ok("Succsesfully added.");

                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }

        }


        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetIngridientByEan() 
        {
            return Ok("ABCD, abcd, aaa");
        }
    }
}
