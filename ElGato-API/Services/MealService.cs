using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.ModelsMongo.Diet;
using ElGato_API.ModelsMongo.Meal;
using ElGato_API.VM.Meal;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Meals;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
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
                        StringId = meal.Id.ToString(),
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        Kcal = meal.MealsMakro.Kcal,
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
                        StringId = meal.Id.ToString(),
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        Kcal = meal.MealsMakro.Kcal,
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
                        StringId = meal.Id.ToString(),
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        Kcal = meal.MealsMakro.Kcal,
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
                        StringId = meal.Id.ToString(),
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        Kcal = meal.MealsMakro.Kcal,
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
                        StringId = meal.Id.ToString(),
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        Kcal = meal.MealsMakro.Kcal,
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

        public async Task<BasicErrorResponse> LikeMeal(string userId, string mealId)
        {
            try
            {
                var filter = Builders<MealLikesDocument>.Filter.Eq(x => x.UserId, userId);
                var doc = await _mealLikesCollection.Find(filter).FirstOrDefaultAsync();

                ObjectId objectId = new ObjectId(mealId);
                var mealFilter = Builders<MealsDocument>.Filter.Eq(x => x.Id, objectId);
                var mealDoc = await _mealsCollection.Find(mealFilter).FirstOrDefaultAsync();

                if (mealDoc == null)
                {
                    return new BasicErrorResponse { Success = false, ErrorMessage = "Meal not found" };
                }

                int likeCounter = mealDoc.LikedCounter;

                if (doc == null)
                {
                    var newMealLikesDocument = new MealLikesDocument
                    {
                        UserId = userId,
                        SavedMeals = new List<string>(),
                        LikedMeals = new List<string> { mealId }
                    };

                    await _mealLikesCollection.InsertOneAsync(newMealLikesDocument);

                    likeCounter += 1;
                    var xmealUpdate = Builders<MealsDocument>.Update.Set(x => x.LikedCounter, likeCounter);
                    await _mealsCollection.UpdateOneAsync(mealFilter, xmealUpdate);

                    return new BasicErrorResponse { Success = true };
                }

                if (doc.LikedMeals.Contains(mealId))
                {
                    var update = Builders<MealLikesDocument>.Update.Pull(x => x.LikedMeals, mealId);
                    await _mealLikesCollection.UpdateOneAsync(filter, update);
                    likeCounter -= 1;
                }
                else
                {
                    var update = Builders<MealLikesDocument>.Update.Push(x => x.LikedMeals, mealId);
                    await _mealLikesCollection.UpdateOneAsync(filter, update);
                    likeCounter += 1;
                }

                var mealUpdate = Builders<MealsDocument>.Update.Set(x => x.LikedCounter, likeCounter);
                await _mealsCollection.UpdateOneAsync(mealFilter, mealUpdate);

                return new BasicErrorResponse { Success = true };

            }
            catch (Exception ex)
            {
                return new BasicErrorResponse { Success = false, ErrorMessage = $"Internal server error {ex.Message}"};
            }
        }

        public async Task<BasicErrorResponse> SaveMeal(string userId, string mealId)
        {
            try
            {
                var filter = Builders<MealLikesDocument>.Filter.Eq(x => x.UserId, userId);
                var doc = await _mealLikesCollection.Find(filter).FirstOrDefaultAsync();

                ObjectId objectId = new ObjectId(mealId);
                var mealFilter = Builders<MealsDocument>.Filter.Eq(x => x.Id, objectId);
                var mealDoc = await _mealsCollection.Find(mealFilter).FirstOrDefaultAsync();

                if (mealDoc == null)
                {
                    return new BasicErrorResponse { Success = false, ErrorMessage = "Meal not found" };
                }

                int saveCounter = mealDoc.SavedCounter;

                if (doc == null)
                {
                    var newMealLikesDocument = new MealLikesDocument
                    {
                        UserId = userId,
                        SavedMeals = new List<string> { mealId },
                        LikedMeals = new List<string>()
                    };

                    await _mealLikesCollection.InsertOneAsync(newMealLikesDocument);

                    saveCounter += 1;
                    var xmealUpdate = Builders<MealsDocument>.Update.Set(x => x.SavedCounter, saveCounter);
                    await _mealsCollection.UpdateOneAsync(mealFilter, xmealUpdate);

                    return new BasicErrorResponse { Success = true };
                }

                if (doc.SavedMeals.Contains(mealId))
                {
                    var update = Builders<MealLikesDocument>.Update.Pull(x => x.SavedMeals, mealId);
                    await _mealLikesCollection.UpdateOneAsync(filter, update);
                    saveCounter -= 1;
                }
                else
                {
                    var update = Builders<MealLikesDocument>.Update.Push(x => x.SavedMeals, mealId);
                    await _mealLikesCollection.UpdateOneAsync(filter, update);
                    saveCounter += 1;
                }

                var mealUpdate = Builders<MealsDocument>.Update.Set(x => x.SavedCounter, saveCounter);
                await _mealsCollection.UpdateOneAsync(mealFilter, mealUpdate);

                return new BasicErrorResponse { Success = true };
            }
            catch (Exception ex) 
            {
                return new BasicErrorResponse { Success = false, ErrorMessage = $"Interna lserver error {ex.Message}" };
            }
        }

        public async Task<(BasicErrorResponse error, List<SimpleMealVMO> res)> Search(string userId, SearchMealVM model)
        {
            List<SimpleMealVMO> res = new List<SimpleMealVMO>();
            Dictionary<string, UserData> users = new Dictionary<string, UserData>();

            try
            {
                var filterBuilder = Builders<MealsDocument>.Filter;
                var filters = new List<FilterDefinition<MealsDocument>>();

                if (!string.IsNullOrEmpty(model.Phrase))
                {
                    var phraseFilter = filterBuilder.Or(
                        filterBuilder.Regex("Name", new BsonRegularExpression(model.Phrase, "i")),
                        filterBuilder.Regex("Ingredients", new BsonRegularExpression(model.Phrase, "i"))
                    );
                    filters.Add(phraseFilter);
                }

                if (model.Nutritions != null)
                {
                    filters.Add(filterBuilder.Gte("MealsMakro.Kcal", model.Nutritions.MinimalCalories));
                    filters.Add(filterBuilder.Lte("MealsMakro.Kcal", model.Nutritions.MaximalCalories));
                    filters.Add(filterBuilder.Gte("MealsMakro.Protein", model.Nutritions.MinimalProtein));
                    filters.Add(filterBuilder.Lte("MealsMakro.Protein", model.Nutritions.MaximalProtein));
                    filters.Add(filterBuilder.Gte("MealsMakro.Fats", model.Nutritions.MinimumFats));
                    filters.Add(filterBuilder.Lte("MealsMakro.Fats", model.Nutritions.MaximumFats));
                    filters.Add(filterBuilder.Gte("MealsMakro.Carbs", model.Nutritions.MinimumCarbs));
                    filters.Add(filterBuilder.Lte("MealsMakro.Carbs", model.Nutritions.MaximumCarbs));
                }

                if (model.SearchTimeRange != null)
                {
                    filters.Add(filterBuilder.Gte("TimeMinutes", model.SearchTimeRange.MinimalTime));
                    filters.Add(filterBuilder.Lte("TimeMinutes", model.SearchTimeRange.MaximumTime));
                }

                var combinedFilters = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

                int skip = (model.PageNumber.Value - 1) * model.Qty.Value;

                List<MealsDocument> meals;
                if (model.SortValue == 0 && string.IsNullOrEmpty(model.Phrase))
                {
                    meals = await _mealsCollection.Aggregate()
                        .Match(combinedFilters)
                        .AppendStage<MealsDocument>("{ $sample: { size: " + model.Qty.Value + " } }")
                        .ToListAsync();
                }
                else
                {
                    var sortDefinition = Builders<MealsDocument>.Sort.Ascending("Name");
                    switch (model.SortValue)
                    {
                        case 1: sortDefinition = Builders<MealsDocument>.Sort.Ascending("Name"); break;
                        case 2: sortDefinition = Builders<MealsDocument>.Sort.Descending("Name"); break;
                        case 3: sortDefinition = Builders<MealsDocument>.Sort.Descending("LikedCounter"); break;
                        case 4: sortDefinition = Builders<MealsDocument>.Sort.Ascending("MealsMakro.Kcal"); break;
                        case 5: sortDefinition = Builders<MealsDocument>.Sort.Descending("MealsMakro.Kcal"); break;
                        case 6: sortDefinition = Builders<MealsDocument>.Sort.Ascending("MealsMakro.Protein"); break;
                        case 7: sortDefinition = Builders<MealsDocument>.Sort.Descending("MealsMakro.Protein"); break;
                        case 8: sortDefinition = Builders<MealsDocument>.Sort.Ascending("MealsMakro.Fats"); break;
                        case 9: sortDefinition = Builders<MealsDocument>.Sort.Descending("MealsMakro.Fats"); break;
                        case 10: sortDefinition = Builders<MealsDocument>.Sort.Ascending("MealsMakro.Carbs"); break;
                        case 11: sortDefinition = Builders<MealsDocument>.Sort.Descending("MealsMakro.Carbs"); break;
                    }

                    meals = await _mealsCollection.Find(combinedFilters)
                        .Sort(sortDefinition)
                        .Skip(skip)
                        .Limit(model.Qty.Value)
                        .ToListAsync();
                }

                var userIds = meals.Select(meal => meal.UserId).Distinct().ToList();

                using (var _context = _contextFactory.CreateDbContext())
                {
                    users = await _context.AppUser
                                        .Where(user => userIds.Contains(user.Id))
                                        .ToDictionaryAsync(user => user.Id, user => new UserData { Name = user.Name, Pfp = user.Pfp });
                }

                var userLikesDoc = await _mealLikesCollection.Find(l => l.UserId == userId).FirstOrDefaultAsync();
                var likedMeals = userLikesDoc?.LikedMeals ?? new List<string>();
                var savedMeals = userLikesDoc?.SavedMeals ?? new List<string>();

                res = meals.Select(meal => new SimpleMealVMO
                {
                    Id = meal.Id,
                    StringId = meal.Id.ToString(),
                    Name = meal.Name,
                    Time = meal.Time,
                    Img = meal.Img,
                    Kcal = meal.MealsMakro.Kcal,
                    SavedCounter = meal.SavedCounter,
                    LikedCounter = meal.LikedCounter,
                    CreatorName = users.ContainsKey(meal.UserId) ? users[meal.UserId].Name : "Unknown",
                    CreatorPfp = users.ContainsKey(meal.UserId) ? users[meal.UserId].Pfp : "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png",
                    Liked = likedMeals.Contains(meal.Id.ToString()),
                    Saved = savedMeals.Contains(meal.Id.ToString())
                }).ToList();

                return (new BasicErrorResponse() { Success = true }, res);

            }
            catch (Exception ex)
            {
                return (new BasicErrorResponse() { Success = false, ErrorMessage = $"error: {ex.Message}" }, res);
            }
        }



        public async Task<(BasicErrorResponse error, List<SimpleMealVMO> res)> GetUserLikedMeals(string userId)
        {
            List<SimpleMealVMO> res = new List<SimpleMealVMO>();

            try
            {
                var filter = Builders<MealLikesDocument>.Filter.Eq(x => x.UserId, userId);
                var doc = await _mealLikesCollection.Find(filter).FirstOrDefaultAsync();
                if (doc == null)
                {
                    return (new BasicErrorResponse() { Success = false, ErrorMessage = "User likes document null." }, res);
                }

                var likedMealIds = doc.LikedMeals.Select(ObjectId.Parse).ToList();
                var mealsFilter = Builders<MealsDocument>.Filter.In(x => x.Id, likedMealIds);
                var likedMeals = await _mealsCollection.Find(mealsFilter).ToListAsync();

                if (likedMeals == null || !likedMeals.Any())
                {
                    return (new BasicErrorResponse() { Success = false, ErrorMessage = "No meals found for liked meals." }, res);
                }

                var userIds = likedMeals.Select(meal => meal.UserId).Distinct().ToList();

                using (var _context = _contextFactory.CreateDbContext())
                {
                    var users = await _context.AppUser
                                              .Where(user => userIds.Contains(user.Id))
                                              .ToDictionaryAsync(user => user.Id, user => new { user.Name, user.Pfp });

                    res = likedMeals.Select(meal => new SimpleMealVMO
                    {
                        Id = meal.Id,
                        StringId = meal.Id.ToString(),
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        Kcal = meal.MealsMakro.Kcal,
                        SavedCounter = meal.SavedCounter,
                        LikedCounter = meal.LikedCounter,
                        CreatorName = users.ContainsKey(meal.UserId) ? users[meal.UserId].Name : "Unknown",
                        CreatorPfp = users.ContainsKey(meal.UserId) ? users[meal.UserId].Pfp : "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png",
                        Liked = true, 
                        Saved = doc.SavedMeals.Contains(meal.Id.ToString()) 
                    }).ToList();
                }

                return (new BasicErrorResponse() { Success = true }, res);
            }
            catch (Exception ex)
            {
                return (new BasicErrorResponse() { Success = false, ErrorMessage = $"Something went wrong: {ex.Message}" }, res);
            }
        }


        public async Task<(BasicErrorResponse error, List<SimpleMealVMO> res)> GetUserSavedMeals(string userId)
        {
            List<SimpleMealVMO> res = new List<SimpleMealVMO>();

            try
            {
                var filter = Builders<MealLikesDocument>.Filter.Eq(x => x.UserId, userId);
                var doc = await _mealLikesCollection.Find(filter).FirstOrDefaultAsync();
                if (doc == null)
                {
                    return (new BasicErrorResponse() { Success = false, ErrorMessage = "User saved document is null." }, res);
                }

                var savedMealIds = doc.SavedMeals.Select(ObjectId.Parse).ToList();
                var mealsFilter = Builders<MealsDocument>.Filter.In(x => x.Id, savedMealIds);
                var savedMeals = await _mealsCollection.Find(mealsFilter).ToListAsync();

                if (savedMeals == null || !savedMeals.Any())
                {
                    return (new BasicErrorResponse() { Success = false, ErrorMessage = "No meals found for saved meals." }, res);
                }

                var userIds = savedMeals.Select(meal => meal.UserId).Distinct().ToList();

                using (var _context = _contextFactory.CreateDbContext())
                {
                    var users = await _context.AppUser
                                              .Where(user => userIds.Contains(user.Id))
                                              .ToDictionaryAsync(user => user.Id, user => new { user.Name, user.Pfp });

                    res = savedMeals.Select(meal => new SimpleMealVMO
                    {
                        Id = meal.Id,
                        StringId = meal.Id.ToString(),
                        Name = meal.Name,
                        Time = meal.Time,
                        Img = meal.Img,
                        Kcal = meal.MealsMakro.Kcal,
                        SavedCounter = meal.SavedCounter,
                        LikedCounter = meal.LikedCounter,
                        CreatorName = users.ContainsKey(meal.UserId) ? users[meal.UserId].Name : "Unknown",
                        CreatorPfp = users.ContainsKey(meal.UserId) ? users[meal.UserId].Pfp : "/pfp-images/e2f56642-a493-4c6d-924b-d3072714646a.png",
                        Liked = doc.LikedMeals.Contains(meal.Id.ToString()), 
                        Saved = true 
                    }).ToList();
                }

                return (new BasicErrorResponse() { Success = true }, res);
            }
            catch (Exception ex)
            {
                return (new BasicErrorResponse() { Success = false, ErrorMessage = $"Something went wrong: {ex.Message}" }, res);
            }
        }


        private bool CheckIfLiked(List<string> liked, string mealId)
        {
            return liked != null && liked.Contains(mealId);
        }

        private int ConvertTimeStringToMinutes(string timeString)
        {
            int totalMinutes = 0;
            string[] timeParts = timeString.ToLower().Split(' ');

            for (int i = 0; i < timeParts.Length; i++)
            {
                if (timeParts[i].Contains("day"))
                {
                    if (int.TryParse(timeParts[i - 1], out int days))
                        totalMinutes += days * 24 * 60;
                }

                if (timeParts[i].Contains("hour") || timeParts[i].Contains("hrs") || timeParts[i].Contains("hr"))
                {
                    if (int.TryParse(timeParts[i - 1], out int hours))
                        totalMinutes += hours * 60;
                }

                if (timeParts[i].Contains("min"))
                {
                    if (int.TryParse(timeParts[i - 1], out int minutes))
                        totalMinutes += minutes;
                }
            }

            return totalMinutes;
        }

        public class UserData
        {
            public string Name { get; set; }
            public string Pfp { get; set; }
        }

    }
}
