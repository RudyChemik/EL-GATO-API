using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    return StatusCode(400, ex.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }

        }


        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetIngridientByEan(string ean) 
        {
            try {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.GetIngridientByEan(ean);
                if (res.ingridient != null)
                    return Ok(res.ingridient);

                return StatusCode(400, res.error.ErrorMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetListOfCorrelatedItemByName(string name)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.GetListOfIngridientsByName(name);

                if (res.error.Success)
                    return Ok(res.ingridients);

                return StatusCode(400, res.error.ErrorMessage);

            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }


        [HttpDelete]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> DeleteMeal(int publicId, DateTime date) 
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
                    var res = await _dietService.DeleteMeal(userId, publicId, date);
                    if (!res.Success)
                        return StatusCode(400, res.ErrorMessage);

                    return Ok();
                }
                catch (Exception ex) 
                {
                    return StatusCode(400, ex.Message);
                }

            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }
    }
}
