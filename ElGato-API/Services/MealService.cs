using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.ModelsMongo.Diet;
using ElGato_API.ModelsMongo.Meal;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Meals;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace ElGato_API.Services
{
    public class MealService : IMealService
    {
        private readonly IMongoCollection<MealsDocument> _mealsCollection;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMongoCollection<MealLikesDocument> _mealLikesCollection;

        public MealService(IMongoDatabase database, IDbContextFactory<AppDbContext> contextFactory)
        {
            _mealsCollection = database.GetCollection<MealsDocument>("MealsDoc");
            _mealLikesCollection = database.GetCollection<MealLikesDocument>("MealsLikeDoc");
            _contextFactory = contextFactory;
        }

        public async Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetByMainCategory(List<string> LikedMeals, List<string> SavedMeals, string? category, int? qty = 5)
        {
            List<SimpleMealVMO> res = new List<SimpleMealVMO>();

            try
            {
                using (var _context = _contextFactory.CreateDbContext())
                {
                    if (string.IsNullOrWhiteSpace(category))
                        return (res, new BasicErrorResponse() { Success = false, ErrorMessage = "Category is required" });

                    var filter = Builders<MealsDocument>.Filter.Regex(meal => meal.Categories, new MongoDB.Bson.BsonRegularExpression(category, "i"));

                    var totalMatchingDocs = await _mealsCollection.CountDocumentsAsync(filter);

                    var random = new Random();
                    int skip = 0;
                    if (totalMatchingDocs > qty)
                    {
                        skip = random.Next(0, (int)totalMatchingDocs - qty.Value);
                    }

                    var meals = await _mealsCollection.Find(filter)
                                                      .Skip(skip)
                                                      .Limit(qty.Value)
                                                      .ToListAsync();

                    var userIds = meals.Select(meal => meal.UserId).Distinct().ToList();

                    var users = await _context.AppUser
                                        .Where(user => userIds.Contains(user.Id))
                                        .ToDictionaryAsync(user => user.Id, user => new { user.Name, user.Pfp });

                    res = meals.Select(meal => new SimpleMealVMO
                    {
                        Id = meal.Id,
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        SavedCounter = meal.SavedCounter,
                        LikedCounter = meal.LikedCounter,
                        CreatorName = users.ContainsKey(meal.UserId) ? users[meal.UserId].Name : "Unknown",
                        CreatorPfp = users.ContainsKey(meal.UserId) ? users[meal.UserId].Pfp : "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png",
                        Liked = CheckIfLiked(LikedMeals, meal.Id.ToString()),
                        Saved = CheckIfLiked(SavedMeals, meal.Id.ToString()),                        
                    }).ToList();
                }

                return (res, new BasicErrorResponse() { Success = true });
            }
            catch (Exception ex)
            {
                return (res, new BasicErrorResponse() { Success = false, ErrorMessage = $"Internal server error {ex.Message}" });
            }
        }

        public async Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetByHighMakro(List<string> LikedMeals, List<string> SavedMeals, string makroComponent, int? qty = 5)
        {
            List<SimpleMealVMO> res = new List<SimpleMealVMO>();

            try
            {
                FilterDefinition<MealsDocument> filter;
                switch (makroComponent.ToLower())
                {
                    case "protein":
                        filter = Builders<MealsDocument>.Filter.Gt(meal => meal.MealsMakro.Protein, 20);
                        break;
                    case "carbs":
                        filter = Builders<MealsDocument>.Filter.Gt(meal => meal.MealsMakro.Carbs, 50);
                        break;
                    case "fats":
                        filter = Builders<MealsDocument>.Filter.Gt(meal => meal.MealsMakro.Fats, 15);
                        break;
                    default:
                        return (res, new BasicErrorResponse() { Success = false, ErrorMessage = "Invalid macro component." });
                }

                var totalMatchingDocs = await _mealsCollection.CountDocumentsAsync(filter);

                var random = new Random();
                int skip = 0;
                if (totalMatchingDocs > qty)
                {
                    skip = random.Next(0, (int)totalMatchingDocs - qty.Value);
                }

                var meals = await _mealsCollection.Find(filter)
                                                  .Skip(skip)
                                                  .Limit(qty.Value)
                                                  .ToListAsync();

                var userIds = meals.Select(meal => meal.UserId).Distinct().ToList();

                using (var _context = _contextFactory.CreateDbContext())
                {
                    var users = await _context.AppUser
                                        .Where(user => userIds.Contains(user.Id))
                                        .ToDictionaryAsync(user => user.Id, user => new { user.Name, user.Pfp });

                    res = meals.Select(meal => new SimpleMealVMO
                    {
                        Id = meal.Id,
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        SavedCounter = meal.SavedCounter,
                        LikedCounter = meal.LikedCounter,
                        CreatorName = users.ContainsKey(meal.UserId) ? users[meal.UserId].Name : "Unknown",
                        CreatorPfp = users.ContainsKey(meal.UserId) ? users[meal.UserId].Pfp : "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png",
                        Liked = CheckIfLiked(LikedMeals, meal.Id.ToString()), 
                        Saved = CheckIfLiked(SavedMeals, meal.Id.ToString()) 
                    }).ToList();
                }

                return (res, new BasicErrorResponse() { Success = true });
            }
            catch (Exception ex)
            {
                return (res, new BasicErrorResponse() { Success = false, ErrorMessage = $"Internal server error {ex.Message}" });
            }
        }

        public async Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetByLowMakro(List<string> LikedMeals, List<string> SavedMeals, string makroComponent, int? qty = 5)
        {
            List<SimpleMealVMO> res = new List<SimpleMealVMO>();

            try
            {
                FilterDefinition<MealsDocument> filter;
                switch (makroComponent.ToLower())
                {
                    case "protein":
                        filter = Builders<MealsDocument>.Filter.Lt(meal => meal.MealsMakro.Protein, 10);
                        break;
                    case "carbs":
                        filter = Builders<MealsDocument>.Filter.Lt(meal => meal.MealsMakro.Carbs, 20);
                        break;
                    case "fats":
                        filter = Builders<MealsDocument>.Filter.Lt(meal => meal.MealsMakro.Fats, 5);
                        break;
                    default:
                        return (res, new BasicErrorResponse() { Success = false, ErrorMessage = "Invalid macro component." });
                }

                var totalMatchingDocs = await _mealsCollection.CountDocumentsAsync(filter);

                var random = new Random();
                int skip = 0;
                if (totalMatchingDocs > qty)
                {
                    skip = random.Next(0, (int)totalMatchingDocs - qty.Value);
                }

                var meals = await _mealsCollection.Find(filter)
                                                  .Skip(skip)
                                                  .Limit(qty.Value)
                                                  .ToListAsync();

                var userIds = meals.Select(meal => meal.UserId).Distinct().ToList();

                using (var _context = _contextFactory.CreateDbContext())
                {
                    var users = await _context.AppUser
                                        .Where(user => userIds.Contains(user.Id))
                                        .ToDictionaryAsync(user => user.Id, user => new { user.Name, user.Pfp });

                    res = meals.Select(meal => new SimpleMealVMO
                    {
                        Id = meal.Id, 
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        SavedCounter = meal.SavedCounter,
                        LikedCounter = meal.LikedCounter,
                        CreatorName = users.ContainsKey(meal.UserId) ? users[meal.UserId].Name : "Unknown",
                        CreatorPfp = users.ContainsKey(meal.UserId) ? users[meal.UserId].Pfp : "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png",
                        Liked = CheckIfLiked(LikedMeals, meal.Id.ToString()), 
                        Saved = CheckIfLiked(SavedMeals, meal.Id.ToString()) 
                    }).ToList();
                }

                return (res, new BasicErrorResponse() { Success = true });
            }
            catch (Exception ex)
            {
                return (res, new BasicErrorResponse() { Success = false, ErrorMessage = $"Internal server error {ex.Message}" });
            }
        }

        public async Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetMostLiked(List<string> LikedMeals, List<string> SavedMeals, int? qty = 5)
        {
            List<SimpleMealVMO> res = new List<SimpleMealVMO>();

            try
            {
                var meals = await _mealsCollection.Find(Builders<MealsDocument>.Filter.Empty)
                                                  .Sort(Builders<MealsDocument>.Sort.Descending(meal => meal.LikedCounter))
                                                  .Limit(qty.Value)
                                                  .ToListAsync();

                var userIds = meals.Select(meal => meal.UserId).Distinct().ToList();

                using (var _context = _contextFactory.CreateDbContext())
                {
                    var users = await _context.AppUser
                                        .Where(user => userIds.Contains(user.Id))
                                        .ToDictionaryAsync(user => user.Id, user => new { user.Name, user.Pfp });

                    res = meals.Select(meal => new SimpleMealVMO
                    {
                        Id = meal.Id,
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        SavedCounter = meal.SavedCounter,
                        LikedCounter = meal.LikedCounter,
                        CreatorName = users.ContainsKey(meal.UserId) ? users[meal.UserId].Name : "Unknown",
                        CreatorPfp = users.ContainsKey(meal.UserId) ? users[meal.UserId].Pfp : "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png",
                        Liked = CheckIfLiked(LikedMeals, meal.Id.ToString()),
                        Saved = CheckIfLiked(SavedMeals, meal.Id.ToString())
                    }).ToList();
                }

                return (res, new BasicErrorResponse() { Success = true });
            }
            catch (Exception ex)
            {
                return (res, new BasicErrorResponse() { Success = false, ErrorMessage = $"Internal server error {ex.Message}" });
            }
        }

        public async Task<(List<SimpleMealVMO> res, BasicErrorResponse error)> GetRandom(List<string> LikedMeals,List<string> SavedMeals,int? qty = 5)
        {
            List<SimpleMealVMO> res = new List<SimpleMealVMO>();

            try
            {
                var meals = await _mealsCollection.Aggregate()
                                                  .Sample(qty.Value)
                                                  .ToListAsync();

                var userIds = meals.Select(meal => meal.UserId).Distinct().ToList();

                using (var _context = _contextFactory.CreateDbContext())
                {
                    var users = await _context.AppUser
                                        .Where(user => userIds.Contains(user.Id))
                                        .ToDictionaryAsync(user => user.Id, user => new { user.Name, user.Pfp });

                    res = meals.Select(meal => new SimpleMealVMO
                    {
                        Id = meal.Id, 
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        SavedCounter = meal.SavedCounter,
                        LikedCounter = meal.LikedCounter,
                        CreatorName = users.ContainsKey(meal.UserId) ? users[meal.UserId].Name : "Unknown",
                        CreatorPfp = users.ContainsKey(meal.UserId) ? users[meal.UserId].Pfp : "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png",
                        Liked = CheckIfLiked(LikedMeals, meal.Id.ToString()),
                        Saved = CheckIfLiked(SavedMeals, meal.Id.ToString()) 
                    }).ToList();
                }

                return (res, new BasicErrorResponse() { Success = true });
            }
            catch (Exception ex)
            {
                return (res, new BasicErrorResponse() { Success = false, ErrorMessage = $"Internal server error {ex.Message}" });
            }
        }


        public async Task<(MealLikesDocument res, BasicErrorResponse error)> GetUserMealLikeDoc(string userId)
        {
            MealLikesDocument res = new MealLikesDocument();

            try
            {
                var filter = Builders<MealLikesDocument>.Filter.Eq(doc => doc.UserId, userId);
                var doc = await _mealLikesCollection.Find(filter).FirstOrDefaultAsync();

                if (doc != null)
                {
                    res = doc;
                    return (res, new BasicErrorResponse() { Success = true });
                }

                return (res, new BasicErrorResponse() { Success = false, ErrorMessage = "User Likes doc not found,," });
            }
            catch (Exception ex) 
            {
                return (res, new BasicErrorResponse() { Success = false, ErrorMessage = $"Internal server error {ex.Message}" });
            }
        }

        private bool CheckIfLiked(List<string> liked, string mealId) 
        {
            return liked != null && liked.Contains(mealId);
        }
       
    }
}
