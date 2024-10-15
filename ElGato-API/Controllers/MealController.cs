using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.VMO.Meals;
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
                List<string> UserLikes = new List<string>();
                List<string> UserSaves = new List<string>();

                string userId = _jwtService.GetUserIdClaim();
                var userLikesDoc = await _mealService.GetUserMealLikeDoc(userId);

                if (userLikesDoc.error.Success) {
                    UserLikes = userLikesDoc.res.LikedMeals;
                    UserSaves = userLikesDoc.res.SavedMeals;
                }

                var tasks = new[]
                {
                    _mealService.GetByMainCategory(UserLikes, UserSaves, "Breakfast", 5),
                    _mealService.GetByMainCategory(UserLikes, UserSaves, "Main", 5),
                    _mealService.GetByMainCategory(UserLikes, UserSaves, "Side", 5),
                    _mealService.GetByHighMakro(UserLikes, UserSaves, "protein"),
                    _mealService.GetByHighMakro(UserLikes, UserSaves, "carbs"),
                    _mealService.GetByLowMakro(UserLikes, UserSaves, "carbs"),
                    _mealService.GetByLowMakro(UserLikes, UserSaves, "fats"),
                    _mealService.GetMostLiked(UserLikes, UserSaves, 5),
                    _mealService.GetRandom(UserLikes, UserSaves, 5)
                };

                var results = await Task.WhenAll(tasks);

                foreach (var (res, error) in results)
                {
                    if (!error.Success)
                    {
                        return StatusCode(500, error.ErrorMessage);
                    }
                }

                StartersVMO starters = new StartersVMO()
                {
                    Breakfast = results[0].res,
                    MainDish = results[1].res,
                    SideDish = results[2].res,
                    HighProtein = results[3].res,
                    HighCarb = results[4].res,
                    LowCarbs = results[5].res,
                    LowFats = results[6].res,
                    MostLiked = results[7].res,
                    All = results[8].res,
                };

                return Ok(starters);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
