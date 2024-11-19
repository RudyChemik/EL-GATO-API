using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.VM.Meal;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Meals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Bson;

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
                    _mealService.GetByMainCategory(userId, UserLikes, UserSaves, "Breakfast", 5),
                    _mealService.GetByMainCategory(userId,UserLikes, UserSaves, "Main", 5),
                    _mealService.GetByMainCategory(userId,UserLikes, UserSaves, "Side", 5),
                    _mealService.GetByHighMakro(userId,UserLikes, UserSaves, "protein", 5),
                    _mealService.GetByHighMakro(userId,UserLikes, UserSaves, "carbs", 5),
                    _mealService.GetByLowMakro(userId,UserLikes, UserSaves, "carbs", 5),
                    _mealService.GetByLowMakro(userId,UserLikes, UserSaves, "fats", 5),
                    _mealService.GetMostLiked(userId,UserLikes, UserSaves, 5),
                    _mealService.GetRandom(userId,UserLikes, UserSaves, 5)
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

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetExtendedStarters(ExtendedStartersVM model)
        {
            try
            {
                List<string> UserLikes = new List<string>();
                List<string> UserSaves = new List<string>();

                string userId = _jwtService.GetUserIdClaim();
                var userLikesDoc = await _mealService.GetUserMealLikeDoc(userId);

                if (userLikesDoc.error.Success)
                {
                    UserLikes = userLikesDoc.res.LikedMeals;
                    UserSaves = userLikesDoc.res.SavedMeals;
                }

                List<SimpleMealVMO> mealList = new List<SimpleMealVMO>();
                switch (model.Type)
                {
                    case "Most Liked":
                        var likedMealRes = await _mealService.GetMostLiked(userId, UserLikes, UserSaves, model.pageSize, model.page);
                        if (!likedMealRes.error.Success)
                            return StatusCode(400, $"Error while fetching liked data {likedMealRes.error.ErrorMessage}");
                        mealList.AddRange(likedMealRes.res);
                        break;
                    case "All":
                        var rndMealRes = await _mealService.GetRandom(userId, UserLikes, UserSaves, model.pageSize, model.page);
                        if (!rndMealRes.error.Success)
                            return StatusCode(400, $"Error while fetching rnd data {rndMealRes.error.ErrorMessage}");
                        mealList.AddRange(rndMealRes.res);
                        break;

                    case "Breakfast":
                        var breakfastRes = await _mealService.GetByMainCategory(userId, UserLikes, UserSaves, "Breakfast", model.pageSize, model.page);
                        if (!breakfastRes.error.Success)
                            return StatusCode(400, $"Error while fetching breakfast data {breakfastRes.error.ErrorMessage}");
                        mealList.AddRange(breakfastRes.res);
                        break;

                    case "Side Dish":
                        var sideDishRes = await _mealService.GetByMainCategory(userId, UserLikes, UserSaves, "Side", model.pageSize, model.page);
                        if (!sideDishRes.error.Success)
                            return StatusCode(400, $"Error while fetching side dish data {sideDishRes.error.ErrorMessage}");
                        mealList.AddRange(sideDishRes.res);
                        break;

                    case "Main Dish":
                        var mainDishRes = await _mealService.GetByMainCategory(userId, UserLikes, UserSaves, "Main", model.pageSize, model.page);
                        if (!mainDishRes.error.Success)
                            return StatusCode(400, $"Error while fetching main dish data {mainDishRes.error.ErrorMessage}");
                        mealList.AddRange(mainDishRes.res);
                        break;

                    case "High Protein":
                        var highProteinRes = await _mealService.GetByHighMakro(userId, UserLikes, UserSaves, "protein", model.pageSize, model.page);
                        if (!highProteinRes.error.Success)
                            return StatusCode(400, $"Error while fetching high protein data {highProteinRes.error.ErrorMessage}");
                        mealList.AddRange(highProteinRes.res);
                        break;

                    case "Low Carbs":
                        var lowCarbsRes = await _mealService.GetByLowMakro(userId, UserLikes, UserSaves, "carbs", model.pageSize, model.page);
                        if (!lowCarbsRes.error.Success)
                            return StatusCode(400, $"Error while fetching low carbs data {lowCarbsRes.error.ErrorMessage}");
                        mealList.AddRange(lowCarbsRes.res);
                        break;

                    case "High Carbs":
                        var highCarbsRes = await _mealService.GetByHighMakro(userId, UserLikes, UserSaves, "carbs", model.pageSize, model.page);
                        if (!highCarbsRes.error.Success)
                            return StatusCode(400, $"Error while fetching high carbs data {highCarbsRes.error.ErrorMessage}");
                        mealList.AddRange(highCarbsRes.res);
                        break;

                    case "Low Fat":
                        var lowFatRes = await _mealService.GetByLowMakro(userId, UserLikes, UserSaves, "fats", model.pageSize, model.page);
                        if (!lowFatRes.error.Success)
                            return StatusCode(400, $"Error while fetching low fat data {lowFatRes.error.ErrorMessage}");
                        mealList.AddRange(lowFatRes.res);
                        break;

                    default:
                        return BadRequest("Invalid type specified");
                };

                return Ok(mealList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetLikedMeals()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();
                List<SimpleMealVMO> res = new List<SimpleMealVMO>();
                var tasks = new[]
                {
                    _mealService.GetUserLikedMeals(userId),
                    _mealService.GetUserSavedMeals(userId),
                };

                var results = await Task.WhenAll(tasks);

                foreach (var (error, ress) in results)
                {
                    if (!error.Success)
                    {
                        return StatusCode(400, error.ErrorMessage);
                    }
                }

                HashSet<SimpleMealVMO> uniqueMeals = new HashSet<SimpleMealVMO>(results[0].res);
                uniqueMeals.UnionWith(results[1].res);

                return Ok(uniqueMeals.ToList());
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Internal server error. {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> LikeMeal(string mealId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _mealService.LikeMeal(userId, mealId);
                if (!res.Success)
                {
                    return StatusCode(400, res.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> SaveMeal(string mealId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _mealService.SaveMeal(userId, mealId);
                if (!res.Success)
                {
                    return StatusCode(400, res.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> Search(SearchMealVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _mealService.Search(userId, model);
                if (!res.error.Success) {
                    return StatusCode(400, $"something went wrong while trying to perform searchn. {res.error.ErrorMessage}");
                }

                return Ok(res.res);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> PublishMeal([FromForm]PublishMealVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();
                if (!ModelState.IsValid) { return StatusCode(400, "Invalid model state. Check VM MODEL"); }

                var res = await _mealService.ProcessAndPublishMeal(userId, model);
                if (!res.error.Success) { return StatusCode(400, $"Error occured: {res.error.ErrorMessage}"); }

                if (res.ach != null) { return Ok(res.ach); }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetOwnRecipes()
        {
            try
            {
                List<string> UserLikes = new List<string>();
                List<string> UserSaves = new List<string>();

                string userId = _jwtService.GetUserIdClaim();
                var userLikesDoc = await _mealService.GetUserMealLikeDoc(userId);

                if (userLikesDoc.error.Success)
                {
                    UserLikes = userLikesDoc.res.LikedMeals;
                    UserSaves = userLikesDoc.res.SavedMeals;
                }

                var res = await _mealService.GetOwnMeals(userId, UserLikes, UserSaves);
                if (!res.error.Success) { return StatusCode(400, $"Error occured: {res.error.ErrorMessage}"); }

                return Ok(res.res);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> DeleteMeal(string mealId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                ObjectId mealIdObj = ObjectId.Parse(mealId);

                var res = await _mealService.DeleteMeal(userId, mealIdObj);
                if (!res.Success) { return StatusCode(400, $"Error occured: {res.ErrorMessage}"); }

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}

