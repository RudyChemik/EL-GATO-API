using ElGato_API.Interfaces;
using ElGato_API.Models.User;
using ElGato_API.ModelsMongo.Diet;
using ElGato_API.ModelsMongo.History;
using ElGato_API.ModelsMongo.Meal;
using ElGato_API.VM.Diet;
using ElGato_API.VMO.Diet;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Questionary;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;
using System.Text.RegularExpressions;


namespace ElGato_API.Services
{
    public class DietService : IDietService
    {
        private readonly IMongoCollection<DietDocument> _dietCollection;
        private readonly IMongoCollection<DietHistoryDocument> _dietHistoryCollection;
        private readonly IMongoCollection<ProductDocument> _productCollection;
        private readonly IMongoCollection<OwnMealsDocument> _ownMealCollection;
        private readonly ILogger<DietService> _logger;

        public DietService(IMongoDatabase database, ILogger<DietService> logger) 
        {
            _dietCollection = database.GetCollection<DietDocument>("DailyDiet");
            _dietHistoryCollection = database.GetCollection<DietHistoryDocument>("DietHistory");
            _productCollection = database.GetCollection<ProductDocument>("products");
            _ownMealCollection = database.GetCollection<OwnMealsDocument>("OwnMealsDoc");
            _logger = logger;
        }

        public async Task<BasicErrorResponse> AddNewMeal(string userId, string mealName, DateTime date)
        {
            int currentMealCount = 0;

            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument != null)
                {
                    if (existingDocument.DailyPlans != null && existingDocument.DailyPlans.Count() >= 6) 
                    {
                        var oldestPlan = existingDocument.DailyPlans.OrderBy(dp => dp.Date).First();
                        await MovePlanToHistory(userId, oldestPlan);

                        var update = Builders<DietDocument>.Update.PullFilter(d => d.DailyPlans, dp => dp.Date == oldestPlan.Date);
                        await _dietCollection.UpdateOneAsync(d => d.UserId == userId, update);
                    }

                    var currentPlan = existingDocument.DailyPlans.FirstOrDefault(dp => dp.Date.Date == date.Date);
                    if (currentPlan != null)
                    {
                        currentMealCount = currentPlan.Meals.Any() ? (currentPlan.Meals.Max(m => m.PublicId) + 1) : 1;

                        var newMeal = new MealPlan() { Name = mealName??("Meal" + currentMealCount), Ingridient = new List<Ingridient>(), PublicId = currentMealCount };
                        currentPlan.Meals.Add(newMeal);

                        var filter = Builders<DietDocument>.Filter.And(
                            Builders<DietDocument>.Filter.Eq(d => d.UserId, userId),
                            Builders<DietDocument>.Filter.Eq("DailyPlans.Date", date.Date)
                        );

                        var update = Builders<DietDocument>.Update.Push("DailyPlans.$.Meals", newMeal);

                        await _dietCollection.UpdateOneAsync(filter, update);
                    }
                    else
                    {
                        var newDailyPlan = new DailyDietPlan
                        {
                            Date = date,
                            Meals = new List<MealPlan> { new MealPlan() { Name = mealName ?? ("Meal" + currentMealCount), Ingridient = new List<Ingridient>(), PublicId = currentMealCount } }
                        };

                        var update = Builders<DietDocument>.Update.Push(d => d.DailyPlans, newDailyPlan);
                        await _dietCollection.UpdateOneAsync(d => d.UserId == userId, update);
                    }

                    return new BasicErrorResponse() { Success = true, ErrorMessage = "Success", ErrorCode = ErrorCodes.None };
                }

                _logger.LogWarning($"User diet document not found while trying to add new meal. UserId: {userId} Date: {date} Method: {nameof(AddNewMeal)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = "User document not found.", ErrorCode = ErrorCodes.NotFound };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add new meal UserId: {userId} MealName: {mealName} Date: {date} Method: {nameof(AddNewMeal)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message, ErrorCode = ErrorCodes.Internal };
            }
        }

        public async Task<BasicErrorResponse> AddIngridientToMeal(string userId, AddIngridientVM model)
        {
            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument == null)
                {
                    _logger.LogWarning($"User diet document not found UserId: {userId} Method: {nameof(AddIngridientToMeal)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "User doc not found", ErrorCode = ErrorCodes.NotFound };
                }

                var filter = Builders<DietDocument>.Filter.And(
                    Builders<DietDocument>.Filter.Eq(d => d.UserId, userId),
                    Builders<DietDocument>.Filter.Eq("DailyPlans.Date", model.date.Date)
                );

                //scalee
                if (model.Ingridient.Prep_For != 100) 
                {
                    ScaleNutrition(model.Ingridient);
                }

                Ingridient ingridient = new Ingridient()
                {
                    Carbs = model.Ingridient.Carbs,
                    Proteins = model.Ingridient.Proteins,
                    Fats = model.Ingridient.Fats,
                    EnergyKcal = model.Ingridient.Kcal,
                    WeightValue = model.WeightValue,
                    PrepedFor = model.Ingridient.Prep_For,
                    publicId = model.Ingridient.Id,
                    Name = model.Ingridient.Name,
                    Servings = model.Ingridient.Servings,

                };

                var update = Builders<DietDocument>.Update.Push("DailyPlans.$.Meals.$[meal].Ingridient", ingridient);

                var arrayFilter = new[]
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("meal.PublicId", model.MealId))
                };

                var updateResult = await _dietCollection.UpdateOneAsync(filter, update, new UpdateOptions { ArrayFilters = arrayFilter });

                if (updateResult.ModifiedCount == 0)
                {
                    _logger.LogWarning($"No matching meals found for given date UserId: {userId} Data: {model} Method: {nameof(AddIngridientToMeal)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "No matching meal found for this date", ErrorCode = ErrorCodes.NotFound };
                }

                return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add ingridient to meal. UserId: {userId} Data: {model} Method: {nameof(AddIngridientToMeal)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message, ErrorCode = ErrorCodes.Internal };
            }
        }

        public async Task<BasicErrorResponse> AddIngredientsToMeals(string userId, AddIngridientsVM model)
        {
            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument == null)
                {
                    _logger.LogWarning($"User diet document not found. UserId: {userId}, Method: {nameof(AddIngredientsToMeals)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "User doc not found", ErrorCode = ErrorCodes.NotFound };
                }

                var filter = Builders<DietDocument>.Filter.And(
                    Builders<DietDocument>.Filter.Eq(d => d.UserId, userId),
                    Builders<DietDocument>.Filter.Eq("DailyPlans.Date", model.date.Date)
                );

                var ingredientsToAdd = new List<Ingridient>();

                foreach (var ingridientVMO in model.Ingridient)
                {
                    if (ingridientVMO.Prep_For != 100)
                    {
                        ScaleNutrition(ingridientVMO);
                    }

                    Ingridient ingridient = new Ingridient()
                    {
                        Carbs = ingridientVMO.Carbs,
                        Proteins = ingridientVMO.Proteins,
                        Fats = ingridientVMO.Fats,
                        EnergyKcal = ingridientVMO.Kcal,
                        Servings = ingridientVMO.Servings,
                        WeightValue = ingridientVMO.WeightValue,
                        PrepedFor = ingridientVMO.Prep_For,
                        publicId = ingridientVMO.Id,
                        Name = ingridientVMO.Name
                    };

                    ingredientsToAdd.Add(ingridient);
                }

                var update = Builders<DietDocument>.Update.PushEach("DailyPlans.$.Meals.$[meal].Ingridient", ingredientsToAdd);

                var arrayFilter = new[]
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("meal.PublicId", model.MealId))
                };

                var updateResult = await _dietCollection.UpdateOneAsync(filter, update, new UpdateOptions { ArrayFilters = arrayFilter });

                if (updateResult.ModifiedCount == 0)
                {
                    _logger.LogWarning($"No matching meals found for given date UserId: {userId} Data: {model} Method: {nameof(AddIngredientsToMeals)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "No matching meal found for this date", ErrorCode = ErrorCodes.NotFound };
                }

                return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to add ingridients to meal. UserId: {userId} Model: {model} Method: {nameof(AddIngredientsToMeals)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message, ErrorCode = ErrorCodes.Internal };
            }
        }


        public async Task<BasicErrorResponse> AddWater(string userId, int water, DateTime date)
        {
            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument == null)
                {
                    _logger.LogWarning($"User diet document not found. UserId: {userId} Method: {nameof(AddWater)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "User doc not found", ErrorCode = ErrorCodes.NotFound };
                }

                var todayPlan = existingDocument.DailyPlans.FirstOrDefault(p => p.Date.Date == date);
                if (todayPlan == null)
                {
                    todayPlan = new DailyDietPlan
                    {
                        Date = date,
                        Water = water,
                        Meals = new List<MealPlan>()
                    };
                    existingDocument.DailyPlans.Add(todayPlan);
                }
                else {
                    todayPlan.Water += water;
                }

                var filter = Builders<DietDocument>.Filter.Eq(d => d.UserId, userId);
                var update = Builders<DietDocument>.Update.Set(d => d.DailyPlans, existingDocument.DailyPlans);
                await _dietCollection.UpdateOneAsync(filter, update);

                return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Failed while trying to add water. UserId: {userId} Date: {date} Method: {nameof(AddWater)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message, ErrorCode = ErrorCodes.Internal };
            }
        }

        public async Task<(IngridientVMO? ingridient, BasicErrorResponse error)> GetIngridientByEan(string ean)
        {
            try
            {
                var filter = Builders<ProductDocument>.Filter.Eq("code", ean);
                var existingDocument = await _productCollection.Find(filter).FirstOrDefaultAsync();

                if (existingDocument != null)
                {

                    if (existingDocument.Nutriments == null)
                    {
                        _logger.LogWarning($"Nutri data not found for product: Ean: {ean} Method: {nameof(GetIngridientByEan)}");
                        return (null, new BasicErrorResponse() { Success = false, ErrorMessage = "No nutritments data found", ErrorCode = ErrorCodes.NotFound });
                    }

                    IngridientVMO ingridient = new IngridientVMO()
                    {
                        Name = existingDocument.Product_name,
                        Id = existingDocument.Id,
                        Fats = existingDocument.Nutriments.Fat,
                        Carbs = existingDocument.Nutriments.Carbs,
                        Proteins = existingDocument.Nutriments.Proteins,
                        Brand = existingDocument.Brands?.Split(',').FirstOrDefault()?.Trim() ?? "",
                        Prep_For = ConvertToDoubleClean(existingDocument.Nutrition_data_prepared_per),
                        Kcal = existingDocument.Nutriments.EnergyKcal,
                    };

                    return (ingridient, new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None });
                }

                return (null, new BasicErrorResponse() { Success = false, ErrorMessage = "No occurencies", ErrorCode = ErrorCodes.NotFound });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Getining ingredient by ean failed. Ean: {ean} Method: {nameof(GetIngridientByEan)}");
                return (null, new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message, ErrorCode = ErrorCodes.Internal });
            }
        }

        public async Task<BasicErrorResponse> AddMealToSavedMeals(string userId, SaveIngridientMealVM model)
        {
            try
            {
                Random rnd = new Random();

                var userOwnMealDoc = await _ownMealCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (userOwnMealDoc == null)
                {
                    OwnMealsDocument ownMealsDocument = new OwnMealsDocument()
                    {
                        UserId = userId,
                        OwnMealsId = new List<string>(),
                        SavedIngMeals = new List<MealPlan>() { new MealPlan() { Name = model.Name, Ingridient = model.Ingridients, PublicId = rnd.Next(1,99999)} }
                    };
                    await _ownMealCollection.InsertOneAsync(ownMealsDocument);
                    return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };
                }

                if(userOwnMealDoc.SavedIngMeals.FirstOrDefault(a=>a.Name == model.Name) != null)
                {
                    _logger.LogInformation($"User tried to save meal with already existen name. UserId: {userId}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "Already saved meal with given name.", ErrorCode = ErrorCodes.AlreadyExists };
                }

                userOwnMealDoc.SavedIngMeals.Add(new MealPlan() { Name = model.Name, Ingridient = model.Ingridients, PublicId = rnd.Next(1, 99999) });
                await _ownMealCollection.ReplaceOneAsync(
                    filter: a => a.UserId == userId,
                    replacement: userOwnMealDoc
                );

                return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Saving meal failed. UserId: {userId} Data: {model} Method: {nameof(AddMealToSavedMeals)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage= ex.Message, ErrorCode=ErrorCodes.Internal };
            }
        }

        public async Task<BasicErrorResponse> AddMealFromSavedMeals(string userId, AddMealFromSavedVM model)
        {
            try
            {
                var userOwnMealDoc = await _ownMealCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (userOwnMealDoc == null || userOwnMealDoc.SavedIngMeals == null)
                {
                    _logger.LogWarning($"Saved meals document not found for user. UserId: {userId} Method: {nameof(AddMealFromSavedMeals)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage="No savedmeals document for given user or the doc is empty.", ErrorCode = ErrorCodes.NotFound };
                }

                var savedMealToAdd = userOwnMealDoc.SavedIngMeals.FirstOrDefault(a=>a.Name == model.Name);
                if (savedMealToAdd == null) 
                {
                    _logger.LogWarning($"User does not have meal with given name. Adding meal to saved failed. UserId: {userId} Data: {model} Method: {nameof(AddMealFromSavedMeals)}");
                    return new BasicErrorResponse(){Success = false, ErrorMessage = "User does not have any saved meal wtih given name", ErrorCode = ErrorCodes.NotFound }; 
                }

                var dailyDietDoc = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (dailyDietDoc == null)
                {
                    _logger.LogWarning($"User daily diet document not found. UserId: {userId} Method: {nameof(AddMealFromSavedMeals)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "User daily diet document not found", ErrorCode = ErrorCodes.NotFound };
                }

                var currentPlan = dailyDietDoc.DailyPlans.FirstOrDefault(dp => dp.Date.Date == model.Date);
                int newMealPublicId = 1;

                if (currentPlan != null)
                {
                    newMealPublicId = currentPlan.Meals.Any() ? (currentPlan.Meals.Max(m => m.PublicId) + 1) : 1;

                    var newMeal = new MealPlan
                    {
                        Name = savedMealToAdd.Name,
                        Ingridient = savedMealToAdd.Ingridient,
                        PublicId = newMealPublicId
                    };

                    var filter = Builders<DietDocument>.Filter.And(
                        Builders<DietDocument>.Filter.Eq(d => d.UserId, userId),
                        Builders<DietDocument>.Filter.Eq("DailyPlans.Date", model.Date.Date)
                    );

                    var update = Builders<DietDocument>.Update.Push("DailyPlans.$.Meals", newMeal);
                    await _dietCollection.UpdateOneAsync(filter, update);
                }
                else
                {
                    var newDailyPlan = new DailyDietPlan
                    {
                        Date = model.Date,
                        Meals = new List<MealPlan>
                        {
                            new MealPlan
                            {
                                Name = savedMealToAdd.Name,
                                Ingridient = savedMealToAdd.Ingridient,
                                PublicId = newMealPublicId
                            }
                        }
                    };

                    var update = Builders<DietDocument>.Update.Push(d => d.DailyPlans, newDailyPlan);
                    await _dietCollection.UpdateOneAsync(d => d.UserId == userId, update);
                }

                return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Adding meal from saved meals to training day failed. UserId: {userId} Data: {model} Method: {nameof(AddMealFromSavedMeals)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message, ErrorCode = ErrorCodes.Internal };
            }
        }

        public async Task<(List<IngridientVMO>? ingridients, BasicErrorResponse error)> GetListOfIngridientsByName(string name)
        {
            try
            {
                var filter = Builders<ProductDocument>.Filter.Regex("product_name", new BsonRegularExpression(name, "i"));
                var projection = Builders<ProductDocument>.Projection
                    .Include("product_name")
                    .Include("nutriments")
                    .Include("nutrition_data_prepared_per")
                    .Include("brands")
                    .Include("_id");

                var findOptions = new FindOptions<ProductDocument, BsonDocument>
                {
                    Projection = projection
                };

                var docList = await _productCollection.Find(filter).Project<ProductDocument>(projection).Limit(10).ToListAsync();

                List<IngridientVMO> ingridients = docList
                    .Where(doc =>
                        !(doc.Nutriments.Fat == 0 && doc.Nutriments.Carbs == 0 && doc.Nutriments.Proteins == 0) &&
                        !(doc.Nutriments.EnergyKcal == 0))
                    .Select(doc => new IngridientVMO
                    {
                        Name = doc.Product_name,
                        Id = doc.Id,
                        Carbs = doc.Nutriments.Carbs,
                        Proteins = doc.Nutriments.Proteins,
                        Fats = doc.Nutriments.Fat,
                        Brand = doc.Brands?.Split(',').FirstOrDefault()?.Trim() ?? "",
                        Prep_For = ConvertToDoubleClean(doc.Nutrition_data_prepared_per),
                        Kcal = doc.Nutriments.EnergyKcal,
                    })
                    .ToList();

                if (ingridients.Count == 0)
                {
                    return (null, new BasicErrorResponse() { Success = false, ErrorMessage = "No results found", ErrorCode = ErrorCodes.NotFound });
                }

                return (ingridients, new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to get ingriedients by name. Name: {name} Method: {nameof(GetListOfIngridientsByName)}");
                return (null, new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message, ErrorCode = ErrorCodes.Internal });
            }

        }

        public async Task<(BasicErrorResponse errorResponse, DietDocVMO model)> GetUserDoc(string userId)
        {
            DietDocVMO model = new DietDocVMO();
            BasicErrorResponse errorResponse = new BasicErrorResponse() { Success = false };
            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument == null) {
                    _logger.LogWarning($"User diet document not found. UserId: {userId} Method: {nameof(GetUserDoc)}");
                    errorResponse.ErrorMessage = "document not found";
                    errorResponse.ErrorCode = ErrorCodes.NotFound;
                    return (errorResponse, model);
                }

                foreach (var dailyPlan in existingDocument.DailyPlans) 
                { 
                    model.DailyPlans.Add(dailyPlan);
                }

                errorResponse.Success = true;
                return (errorResponse, model);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Failed while trying to retrice user diet document. UserId: {userId} Method: {nameof(GetUserDoc)}");
                errorResponse.ErrorMessage = ex.Message;
                errorResponse.ErrorCode = ErrorCodes.Internal;
                return (errorResponse, model);
            }
        }

        public async Task<(BasicErrorResponse errorResponse, DietDayVMO model)> GetUserDietDay(string userId, DateTime date)
        {
            BasicErrorResponse errorResponse = new BasicErrorResponse() { Success = false };
            DietDayVMO model = new DietDayVMO();

            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument == null)
                {
                    _logger.LogWarning($"User diet document not found. UserId: {userId} Method: {nameof(GetUserDietDay)}");
                    errorResponse.ErrorMessage = "User diet document not found";
                    errorResponse.ErrorCode = ErrorCodes.NotFound;
                    return (errorResponse, model);
                }

                var ownDocumentDoc = await _ownMealCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();

                var dailyPlan = existingDocument.DailyPlans.FirstOrDefault(a => a.Date == date);
                if (dailyPlan != null)
                {
                    model.Date = date;
                    model.Water = dailyPlan.Water;
                    model.Meals = dailyPlan.Meals.Select(meal => new MealPlanVMO
                    {
                        Name = meal.Name,
                        PublicId = meal.PublicId,
                        IsSaved = ownDocumentDoc?.SavedIngMeals?.Any(savedMeal =>
                            savedMeal.Name == meal.Name &&
                            AreIngredientsEqual(savedMeal.Ingridient, meal.Ingridient)) ?? false,
                        Ingridient = meal.Ingridient
                    }).ToList();

                    model.CalorieCounter = new DailyCalorieCount();

                    foreach (var meal in model.Meals)
                    {
                        foreach (var ingr in meal.Ingridient)
                        {
                            double factor = ingr.WeightValue / ingr.PrepedFor;
                            model.CalorieCounter.Protein += (ingr.Proteins * factor);
                            model.CalorieCounter.Fats += (ingr.Fats * factor);
                            model.CalorieCounter.Carbs += (ingr.Carbs * factor);
                            model.CalorieCounter.Kcal += (ingr.EnergyKcal * factor);
                        }
                    }
                    errorResponse.Success = true;
                    errorResponse.ErrorCode = ErrorCodes.None;
                }
                else
                {
                    _logger.LogWarning($"Couldnt find diet doc for user with specified date. Date: {date} UserId: {userId} Method: {nameof(GetUserDietDay)}");
                    errorResponse.ErrorMessage = "Diet plan for the specified date not found";
                    errorResponse.ErrorCode = ErrorCodes.NotFound;
                    return (errorResponse, model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to get user daily diet doc. UserId: {userId} Method: {nameof(GetUserDietDay)}");
                errorResponse.ErrorMessage = ex.Message;
                errorResponse.ErrorCode = ErrorCodes.Internal;
                return (errorResponse, model);
            }

            return (errorResponse, model);
        }

        private bool AreIngredientsEqual(List<Ingridient> ingredients1, List<Ingridient> ingredients2)
        {
            if (ingredients1.Count != ingredients2.Count)
                return false;

            foreach (var ingr1 in ingredients1)
            {
                var matchingIngr = ingredients2.FirstOrDefault(ingr2 =>
                    ingr2.Name == ingr1.Name &&
                    ingr2.WeightValue == ingr1.WeightValue);

                if (matchingIngr == null)
                    return false;
            }

            return true;
        }

        public async Task<(BasicErrorResponse errorResponse, List<MealPlan>? model)> GetSavedMeals(string userId)
        {
            try
            {
                var userOwnMealsDoc = await _ownMealCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
                if(userOwnMealsDoc == null)
                {
                    OwnMealsDocument ownMealsDocument = new OwnMealsDocument()
                    {
                        UserId = userId,
                        OwnMealsId = new List<string>(),
                        SavedIngMeals = new List<MealPlan>()  
                    };
                    await _ownMealCollection.InsertOneAsync(ownMealsDocument);
                    return (new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None }, null);
                }

                return (new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None }, userOwnMealsDoc.SavedIngMeals ?? null);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Failed while trying to get user saved meals. UserId: {userId} Method: {nameof(GetSavedMeals)}");
                return (new BasicErrorResponse { ErrorMessage = ex.Message, Success = false, ErrorCode = ErrorCodes.Internal }, null);
            }
        }

        public async Task<BasicErrorResponse> DeleteMeal(string userId, int publicId, DateTime date)
        {
            try
            {
                var filter = Builders<DietDocument>.Filter.And(
                    Builders<DietDocument>.Filter.Eq(d => d.UserId, userId),
                    Builders<DietDocument>.Filter.Eq("DailyPlans.Date", date.Date)
                );

                var update = Builders<DietDocument>.Update.PullFilter("DailyPlans.$[dailyPlan].Meals",
                    Builders<MealPlan>.Filter.Eq(m => m.PublicId, publicId));

                var updateOptions = new UpdateOptions
                {
                    ArrayFilters = new List<ArrayFilterDefinition>
                    {
                        new BsonDocumentArrayFilterDefinition<BsonDocument>(
                            new BsonDocument("dailyPlan.Date", date.Date)
                        )
                    }
                };

                var res = await _dietCollection.UpdateOneAsync(filter, update, updateOptions);

                if (res.ModifiedCount > 0)
                    return new BasicErrorResponse { Success = true };

                _logger.LogError($"Couldnt remove meal. Mongo update failed. UserId: {userId} PublicId: {publicId} Date: {date} Method: {nameof(DeleteMeal)}");
                return new BasicErrorResponse { Success = false, ErrorMessage = "Meal not found or could not be removed", ErrorCode = ErrorCodes.NotFound };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to delete meal. UserId: {userId} PublicId: {publicId} Date: {date} Method: {nameof(DeleteMeal)}");
                return new BasicErrorResponse { ErrorMessage = ex.Message, Success = false, ErrorCode= ErrorCodes.Internal };
            }
        }


        public async Task<BasicErrorResponse> DeleteIngridientFromMeal(string userId, RemoveIngridientVM model)
        {
            try
            {
                var filter = Builders<DietDocument>.Filter.And(
                    Builders<DietDocument>.Filter.Eq(d => d.UserId, userId),
                    Builders<DietDocument>.Filter.Eq("DailyPlans.Date", model.Date.Date),
                    Builders<DietDocument>.Filter.Eq("DailyPlans.Meals.PublicId", model.MealPublicId)
                );

                var dietDocument = await _dietCollection.Find(filter).FirstOrDefaultAsync();

                if (dietDocument == null)
                {
                    _logger.LogWarning($"Meal or ingriedient not found while trying to delete. UserId: {userId} Data: {model} Method: {nameof(DeleteIngridientFromMeal)}");
                    return new BasicErrorResponse { ErrorMessage = "Meal or ingredient not found.", Success = false, ErrorCode = ErrorCodes.NotFound };
                }


                var dailyPlan = dietDocument.DailyPlans.FirstOrDefault(dp => dp.Date == model.Date.Date);
                if (dailyPlan == null)
                {
                    _logger.LogWarning($"User daily diet document not found. UserId: {userId} Method: {nameof(DeleteIngridientFromMeal)}");
                    return new BasicErrorResponse { ErrorMessage = "Daily plan not found", Success = false, ErrorCode = ErrorCodes.NotFound };
                }

                var meal = dailyPlan.Meals.FirstOrDefault(m => m.PublicId == model.MealPublicId);
                if (meal == null)
                {
                    _logger.LogWarning($"Meal not found while trying to perform delete action. UserId: {userId} MealPublicId: {model.MealPublicId} Method: {nameof(DeleteIngridientFromMeal)}");
                    return new BasicErrorResponse { ErrorMessage = "Meal not found", Success = false, ErrorCode = ErrorCodes.NotFound };
                }


                var ingredientToRemove = meal.Ingridient
                    .FirstOrDefault(i => i.publicId == model.IngridientId &&
                                         i.WeightValue == model.WeightValue &&
                                         i.Name == model.IngridientName);

                if (ingredientToRemove == null)
                {
                    _logger.LogWarning($"Ingridient not found. UserId: {userId} IngridientPublicId: {model.IngridientId} Method: {nameof(DeleteIngridientFromMeal)}");
                    return new BasicErrorResponse { ErrorMessage = "Ingridient not found", Success = false, ErrorCode = ErrorCodes.NotFound };
                }

                meal.Ingridient.Remove(ingredientToRemove);

                var update = Builders<DietDocument>.Update.Set("DailyPlans.$[dailyPlan].Meals.$[meal].Ingridient", meal.Ingridient);

                var arrayFilters = new[]
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("dailyPlan.Date", model.Date.Date)),
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("meal.PublicId", model.MealPublicId))
                };

                var updateRes = await _dietCollection.UpdateOneAsync(
                    filter,
                    update,
                    new UpdateOptions { ArrayFilters = arrayFilters }
                );

                if (updateRes.ModifiedCount == 0)
                {
                    _logger.LogError($"Mongo db failed to update. UserId: {userId} Data: {model} Method: {nameof(DeleteIngridientFromMeal)}");
                    return new BasicErrorResponse { ErrorMessage = "Failed to update", Success = false, ErrorCode = ErrorCodes.Failed };
                }

                return new BasicErrorResponse { Success = true, ErrorCode = ErrorCodes.None };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to remove ingridient from meal. UserId: {userId} Data: {model} Method: {nameof(DeleteIngridientFromMeal)}");
                return new BasicErrorResponse { ErrorMessage = ex.Message, Success = false, ErrorCode = ErrorCodes.Internal };
            }
        }

        public async Task<BasicErrorResponse> RemoveMealFromSaved(string userId, string name)
        {
            try
            {
                var savedMealDoc = await _ownMealCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (savedMealDoc == null || savedMealDoc.SavedIngMeals == null)
                {
                    _logger.LogWarning($"User diet document not found. UserId: {userId} Method: {nameof(RemoveMealFromSaved)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "User document with saved meals not found.", ErrorCode = ErrorCodes.NotFound };
                }

                var mealToRemove = savedMealDoc.SavedIngMeals.Find(a => a.Name == name);
                if(mealToRemove == null)
                {
                    _logger.LogWarning($"Meal not found while trying to remove from saved. UserId: {userId} MealName: {name} Method: {nameof(RemoveMealFromSaved)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "Meal not found in saved meals", ErrorCode = ErrorCodes.NotFound };
                }

                savedMealDoc.SavedIngMeals.Remove(mealToRemove);

                var updateRes = await _ownMealCollection.ReplaceOneAsync(
                    a => a.UserId == userId,
                    savedMealDoc
                );

                if (updateRes.ModifiedCount > 0)
                {
                    return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };
                }

                _logger.LogError($"Mongo update error while trying to remove meal from saved. UserId: {userId} MealName: {name} Method: {nameof(RemoveMealFromSaved)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = "Something went wrong while removing meal.", ErrorCode = ErrorCodes.Failed };
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Failed while trying to remove meal from saved. UserId: {userId} MealName: {name} Method: {nameof(RemoveMealFromSaved)}");
                return new BasicErrorResponse() { ErrorMessage = ex.Message, Success = false, ErrorCode = ErrorCodes.Internal };
            }
        }

        public async Task<BasicErrorResponse> DeleteMealsFromSaved(string userId, DeleteSavedMealsVM model)
        {
            try
            {
                var ownMealsDoc = await _ownMealCollection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
                if (ownMealsDoc == null || ownMealsDoc.SavedIngMeals == null)
                {
                    _logger.LogWarning($"User meal doc not found for the user. UserId: {userId} Method: {nameof(DeleteMealsFromSaved)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "Saved meals document not found for the user", ErrorCode = ErrorCodes.NotFound };
                }

                foreach (var mealName in model.SavedMealsNames) 
                { 
                    var mealToDelete = ownMealsDoc.SavedIngMeals.FirstOrDefault(a=>a.Name == mealName);
                    if (mealToDelete == null)
                    {
                        _logger.LogWarning($"Meal not found. UserId: {userId} MealName: {mealName} Method: {nameof(DeleteMealsFromSaved)}");
                        return new BasicErrorResponse() { Success = false, ErrorMessage = "Saved meal with given name not found.", ErrorCode = ErrorCodes.NotFound };
                    }
                    ownMealsDoc.SavedIngMeals.Remove(mealToDelete);
                }

                var removeRes = await _ownMealCollection.ReplaceOneAsync(
                    a => a.UserId == userId,
                    ownMealsDoc
                );

                if (!removeRes.IsAcknowledged || removeRes.ModifiedCount == 0)
                {
                    _logger.LogError($"Mongo update failed while trying to remove meals from saved. UserId: {userId} Data: {model} Method: Method: {nameof(DeleteMealsFromSaved)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "Failed to remove meals", ErrorCode = ErrorCodes.Failed };
                }

                return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };

            }
            catch (Exception ex) 
            {
                _logger.LogError($"Failed while trying to remove meals from saved. UserId: {userId} Data: {model} Method: {nameof(DeleteMealsFromSaved)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message, ErrorCode = ErrorCodes.Internal };
            }
        }

        public async Task<BasicErrorResponse> UpdateMealName(string userId, UpdateMealNameVM model)
        {
            try
            {
                var filter = Builders<DietDocument>.Filter.And(
                    Builders<DietDocument>.Filter.Eq(d => d.UserId, userId),
                    Builders<DietDocument>.Filter.Eq("DailyPlans.Date", model.Date.Date)
                );

                var update = Builders<DietDocument>.Update.Set("DailyPlans.$[dailyPlan].Meals.$[meal].Name", model.Name);

                var updateOptions = new UpdateOptions
                {
                    ArrayFilters = new List<ArrayFilterDefinition>
                    {
                        new BsonDocumentArrayFilterDefinition<BsonDocument>(
                            new BsonDocument("dailyPlan.Date", model.Date.Date)
                        ),
                        new BsonDocumentArrayFilterDefinition<BsonDocument>(
                            new BsonDocument("meal.PublicId", model.MealPublicId)
                        )
                    }
                };

                var res = await _dietCollection.UpdateOneAsync(filter, update, updateOptions);

                if (res.ModifiedCount > 0)
                {
                    return new BasicErrorResponse { Success = true, ErrorCode = ErrorCodes.None };
                }

                _logger.LogError($"Mongo update failed. UserId: {userId} Data: {model}, Method: {nameof(UpdateMealName)}");
                return new BasicErrorResponse { Success = false, ErrorMessage = "Meal not found or could not be updated", ErrorCode = ErrorCodes.NotFound };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to update meal name. UserId: {userId} Data: {model}, Method: {nameof(UpdateMealName)}");
                return new BasicErrorResponse { ErrorMessage = ex.Message, Success = false, ErrorCode = ErrorCodes.NotFound };
            }
        }
        public async Task<BasicErrorResponse> UpdateIngridientWeightValue(string userId, UpdateIngridientVM model)
        {
            try
            {
                var filter = Builders<DietDocument>.Filter.And(
                    Builders<DietDocument>.Filter.Eq(d => d.UserId, userId),
                    Builders<DietDocument>.Filter.Eq("DailyPlans.Date", model.Date.Date),
                    Builders<DietDocument>.Filter.Eq("DailyPlans.Meals.PublicId", model.MealPublicId)
                );

                var dietDocument = await _dietCollection.Find(filter).FirstOrDefaultAsync();

                if (dietDocument == null)
                {
                    _logger.LogWarning($"Meal or ingridient not found. UserId: {userId} Data: {model} Method: {nameof(UpdateIngridientWeightValue)}");
                    return new BasicErrorResponse { ErrorMessage = "Meal or ingridient not found", Success = false, ErrorCode = ErrorCodes.NotFound };
                }

                var dailyPlan = dietDocument.DailyPlans.FirstOrDefault(dp => dp.Date == model.Date.Date);
                if (dailyPlan == null)
                {
                    _logger.LogWarning($"User daily diet document not found. UserId: {userId} Method: {nameof(UpdateIngridientWeightValue)}");
                    return new BasicErrorResponse { ErrorMessage = "Daily plan not found", Success = false, ErrorCode = ErrorCodes.NotFound };
                }

                var meal = dailyPlan.Meals.FirstOrDefault(m => m.PublicId == model.MealPublicId);
                if (meal == null)
                {
                    _logger.LogWarning($"Meal not found. UserId: {userId} MealPublicId: {model.MealPublicId} Method: {nameof(UpdateIngridientWeightValue)}");
                    return new BasicErrorResponse { ErrorMessage = "Meal not found", Success = false, ErrorCode = ErrorCodes.NotFound };
                }

                var ingredientToUpdate = meal.Ingridient
                    .FirstOrDefault(i => i.publicId == model.IngridientId &&
                                         i.WeightValue == model.IngridientWeightOld &&
                                         i.Name == model.IngridientName);

                if (ingredientToUpdate == null)
                {
                    _logger.LogWarning($"Ingridient not found. UserId: {userId} Data: {model}, Method: {nameof(UpdateIngridientWeightValue)}");
                    return new BasicErrorResponse { ErrorMessage = "ingridient not found", Success = false, ErrorCode = ErrorCodes.NotFound };
                }

                ingredientToUpdate.WeightValue = model.IngridientWeightNew;

                var updateResult = await _dietCollection.ReplaceOneAsync(
                    filter,
                    dietDocument
                );

                if (updateResult.MatchedCount == 0)
                {
                    _logger.LogError($"Mongo update failed while trying to update ingridient weight. UserId: {userId} Data: {model}, Method: {nameof(UpdateIngridientWeightValue)}");
                    return new BasicErrorResponse { ErrorMessage = "Failed to update ingridient", Success = false, ErrorCode = ErrorCodes.Failed };
                }

                return new BasicErrorResponse { Success = true, ErrorCode = ErrorCodes.None };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to update ingridient weight. UserId: {userId} Data: {model}, Method: {nameof(UpdateIngridientWeightValue)}");
                return new BasicErrorResponse { ErrorMessage = ex.Message, Success = false, ErrorCode = ErrorCodes.Internal };
            }
        }


        public async Task<BasicErrorResponse> UpdateSavedMealIngridientWeight(string userId, UpdateSavedMealWeightVM model)
        {
            try
            {
                var ownMealDoc = await _ownMealCollection.Find(a=>a.UserId == userId).FirstOrDefaultAsync();
                if (ownMealDoc == null || ownMealDoc.SavedIngMeals == null)
                {
                    _logger.LogWarning($"Saved meal document not found. UserId: {userId} Method: {nameof(UpdateSavedMealIngridientWeight)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "User saved meal document does not exist.", ErrorCode = ErrorCodes.NotFound };
                }

                var mealToUpdate = ownMealDoc.SavedIngMeals.FirstOrDefault(a=>a.Name == model.MealName);
                if(mealToUpdate == null) { return new BasicErrorResponse() { Success = false, ErrorMessage = "Saved meal with given meal name does not exists", ErrorCode = ErrorCodes.NotFound }; }

                var ingridientToUpdate = mealToUpdate.Ingridient.FirstOrDefault(a => a.Name == model.IngridientName && a.publicId == model.PublicId);
                if (ingridientToUpdate == null) { return new BasicErrorResponse() { Success = false, ErrorMessage = "Ingridient for update does not exist." , ErrorCode = ErrorCodes.NotFound }; }

                ingridientToUpdate.WeightValue = model.NewWeight;

                var updateRes = await _ownMealCollection.ReplaceOneAsync(
                    a => a.UserId == userId,
                    ownMealDoc
                );

                if (!updateRes.IsAcknowledged || updateRes.ModifiedCount == 0)
                {
                    _logger.LogError($"Mongo update failed while trying to update saved meal ingridient data. UserId: {userId} Data: {model} Method: {nameof(UpdateSavedMealIngridientWeight)}");
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "Failed to update the ingredient weighy", ErrorCode = ErrorCodes.Failed };
                }

                return new BasicErrorResponse() { Success = true, ErrorCode = ErrorCodes.None };

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed while trying to update saved meal ingridient data. UserId: {userId} Data: {model} Method: {nameof(UpdateSavedMealIngridientWeight)}");
                return new BasicErrorResponse() { Success = false, ErrorMessage = $"{ex.Message}", ErrorCode = ErrorCodes.Internal };
            }
        }


        //calcs
        public CalorieIntakeVMO CalculateCalories(QuestionaryVM questionary)
        {
            CalorieIntakeVMO output = new CalorieIntakeVMO();

            if (questionary != null)
            {
                double bmr = calculateBMR(questionary.Woman, questionary.Weight, questionary.Height, questionary.Age);
                if (bmr == 0)
                    return output;

                var tmeMultiplyier = calculateTME(questionary.TrainingDays, questionary.DailyTimeSpendWorking, questionary.JobType);
                bmr *= tmeMultiplyier;

                if (questionary.BodyType == 1)
                {
                    bmr *= 1.05;
                }
                if (questionary.BodyType == 2)
                {
                    bmr *= 0.95;
                }


                if (questionary.Sleep == 7)
                {
                        bmr *= 0.98;
                }
                if (questionary.Sleep == 6)
                {
                        bmr *= 0.96;
                }
                if (questionary.Sleep < 6)
                {
                        bmr *= 0.94;
                }
                if (questionary.Sleep > 10) {
                        bmr *= 0.96;
                }

                /*GOAL + PEACE!*/
                switch (questionary.Goal) {
                    case 1:

                        break;
                    case 2:

                        break;
                    case 3:

                        break;              
                }

                // 500/-500? 

                var makros = getMakros(bmr);

                output.Kcal = Math.Round(bmr);
                output.Protein = Math.Round(makros.Protein);
                output.Carbs = Math.Round(makros.Carbs);
                output.Fat = Math.Round(makros.Fats);
                output.Kj = output.Kcal * 4.184;
            }

            return output;
        }
       

        private double calculateBMR(bool isWoman, double Weight, double Height, short Age)
        {
            double genderValue = 88.362;
            double genderValueWeight = 13.397;
            double genderValueHeight = 4.799;
            double genderValueAge = 5.677;

            if (isWoman)
            {
                genderValue = 447.593;
                genderValueWeight = 9.247;
                genderValueHeight = 3.098;
                genderValueAge = 4.330;
            }

            if (Weight == 0 || Height == 0 || Age == 0)
            {
                return 0;
            }

            return genderValue + (genderValueWeight * Weight) + (genderValueHeight * Height) - (genderValueAge * Age);

        }

        private double calculateTME(int trainingDays, int walkingTime, int jobType)
        {
            double tme = 0;

            if (trainingDays == 0)
            {
                tme += 1.2;
            }
            else if (trainingDays > 0 && trainingDays <= 3)
            {
                tme += 1.375;
            }
            else if (trainingDays > 3 && trainingDays <= 5)
            {
                tme += 1.55;
            }
            else
            {
                tme += 1.7;
            }

            if (walkingTime == 0)
            {
                tme += 0.02;
            }
            else if (walkingTime == 1) {
                tme += 0.05;
            }
            else
            {
                tme += 0.08;
            }


            if (jobType == 2)
            {
                tme += 0.2;
            }
            else if (jobType == 1)
            {
                tme += 0.1;
            }

            return tme;
        }

        private Makros getMakros(double bmr)
        {
            Makros makros = new Makros();

            makros.Protein = (bmr * 0.23) / 4;
            makros.Carbs = (bmr * 0.54) / 4;
            makros.Fats = (bmr * 0.23) / 9;

            return makros;
        }

        private async Task MovePlanToHistory(string userId, DailyDietPlan oldestPlan)
        {
            var historyDocument = await _dietHistoryCollection.Find(h => h.UserId == userId).FirstOrDefaultAsync();
            if (historyDocument == null)
            {
                historyDocument = new DietHistoryDocument
                {
                    UserId = userId,
                    DailyPlans = new List<DailyDietPlan>()
                };
                await _dietHistoryCollection.InsertOneAsync(historyDocument);
            }

            var update = Builders<DietHistoryDocument>.Update.Push(h => h.DailyPlans, oldestPlan);
            await _dietHistoryCollection.UpdateOneAsync(h => h.UserId == userId, update);
        }

        private static double ConvertToDoubleClean(string? input)
        {
            if (input.IsNullOrEmpty())
            {
                return 100;
            }
            string cleanInput = Regex.Replace(input, @"[^0-9\.-]", "");
            try
            {
                return Convert.ToDouble(cleanInput, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 100;
            }
        }

        private void ScaleNutrition(IngridientVMO ingridient)
        {
            double scalingFactor = 100 / ingridient.Prep_For;

            ingridient.Carbs *= scalingFactor;
            ingridient.Proteins *= scalingFactor;
            ingridient.Fats *= scalingFactor;
            ingridient.Kcal *= scalingFactor;
        }

    }

    public class Makros
    {
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
    }
}

