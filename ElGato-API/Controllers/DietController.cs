using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.VM.Diet;
using ElGato_API.VMO.ErrorResponse;
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
                    return StatusCode(400, ex.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }

        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> AddIngridientToMeal([FromBody] AddIngridientVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.AddIngridientToMeal(userId, model);
                if (!res.Success)
                    return StatusCode(400, res?.ErrorMessage ?? "Something went wrong");

                return Ok();

            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> AddIngriedientsToMeal([FromBody] AddIngridientsVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.AddIngredientsToMeals(userId, model);
                if (!res.Success)
                    return StatusCode(400, res?.ErrorMessage ?? "Something went wrong");

                return Ok();
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An internal server error. {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> AddWater(int water, DateTime date)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.AddWater(userId, water, date);
                if (!res.Success)
                    return StatusCode(400, res?.ErrorMessage ?? "Something went wrong");

                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> AddMealToSavedMeals(SaveIngridientMealVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.AddMealToSavedMeals(userId, model);
                if (!res.Success)
                {
                    return StatusCode(400, new
                    {
                        message = res.ErrorMessage,
                        errorCode = res.ErrorCode,
                    });
                }

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> AddMealFromSaved([FromBody] AddMealFromSavedVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();
                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "model state not valid");
                }

                var res = await _dietService.AddMealFromSavedMeals(userId, model);

                if (!res.Success)
                    return StatusCode(400, $"Something went wrong. {res.ErrorMessage}");
                

                return Ok();
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An internal server error occured. {ex.Message}");
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetUserDietDoc()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.GetUserDoc(userId);
                if (!res.errorResponse.Success)
                    return BadRequest(res.errorResponse?.ErrorMessage ?? "Something went wrong");

                return Ok(res.model);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetUserDietDay(DateTime date)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.GetUserDietDay(userId, date);
                if (!res.errorResponse.Success)
                    return BadRequest(res.errorResponse?.ErrorMessage ?? "something went wrong");

                return Ok(res.model);
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

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetSavedMeals()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _dietService.GetSavedMeals(userId);
                if (!res.errorResponse.Success)
                {
                    return StatusCode(400, $"An error occured, {res.errorResponse.ErrorMessage}");
                }

                return Ok(res.model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error occured. {ex.Message}");
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

        [HttpDelete]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> RemoveIngridientFromMeal([FromBody] RemoveIngridientVM model) 
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.DeleteIngridientFromMeal(userId, model);
                if (!res.Success)
                    return BadRequest(res.ErrorMessage);

                return Ok();

            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }

        [HttpDelete]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> RemoveMealFromSavedMeals(string mealName)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.RemoveMealFromSaved(userId, mealName);
                if (!res.Success)
                    return BadRequest(res.ErrorMessage);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occured. {ex.Message}");
            }
        }

        [HttpPatch]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> UpdateIngridientWeightValue([FromBody] UpdateIngridientVM model) 
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.UpdateIngridientWeightValue(userId, model);
                if (!res.Success)
                    return BadRequest(res.ErrorMessage);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }

        [HttpPatch]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> UpdateMealName([FromBody] UpdateMealNameVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.UpdateMealName(userId, model);
                if (!res.Success)
                    return BadRequest(res.ErrorMessage);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred. {ex.Message}");
            }
        }

        [HttpPatch]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> UpdateSavedMealIngridientWeight([FromBody]UpdateSavedMealWeightVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, "questionary model state not valid");
                }

                var res = await _dietService.UpdateSavedMealIngridientWeight(userId, model);
                if (!res.Success) { return BadRequest(res.ErrorMessage); }

                return Ok();
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An internal server error occurred. {ex.Message}");
            }
        }

    }
}
