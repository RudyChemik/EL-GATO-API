using ElGato_API.Interfaces;
using ElGato_API.Models.User;
using ElGato_API.ModelsMongo.Diet;
using ElGato_API.ModelsMongo.Diet.History;
using ElGato_API.VM.Diet;
using ElGato_API.VMO.Diet;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Questionary;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ElGato_API.Services
{
    public class DietService : IDietService
    {
        private readonly IMongoCollection<DietDocument> _dietCollection;
        private readonly IMongoCollection<DietHistoryDocument> _dietHistoryCollection;
        private readonly IMongoCollection<ProductDocument> _productCollection;

        public DietService(IMongoDatabase database) 
        {
            _dietCollection = database.GetCollection<DietDocument>("DailyDiet");
            _dietHistoryCollection = database.GetCollection<DietHistoryDocument>("DietHistory");
            _productCollection = database.GetCollection<ProductDocument>("products");
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

                    return new BasicErrorResponse() { Success = true, ErrorMessage = "Success" };
                }

                return new BasicErrorResponse() { Success = false, ErrorMessage = "User document not found." };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<BasicErrorResponse> AddIngridientToMeal(string userId, AddIngridientVM model)
        {
            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument == null)
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "User doc not found" };

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

                };

                var update = Builders<DietDocument>.Update.Push("DailyPlans.$.Meals.$[meal].Ingridient", ingridient);

                var arrayFilter = new[]
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("meal.PublicId", model.MealId))
                };

                var updateResult = await _dietCollection.UpdateOneAsync(filter, update, new UpdateOptions { ArrayFilters = arrayFilter });

                if (updateResult.ModifiedCount == 0)
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "No matching meal found for this date" };

                return new BasicErrorResponse() { Success = true };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<BasicErrorResponse> AddWater(string userId, int water, DateTime date)
        {
            try
            {
                var existingDocument = await _dietCollection.Find(d => d.UserId == userId).FirstOrDefaultAsync();
                if (existingDocument == null)
                    return new BasicErrorResponse() { Success = false, ErrorMessage = "User doc not found" };

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

                return new BasicErrorResponse() { Success = true };

            }
            catch (Exception ex) 
            {
                return new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message };
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
                        return (null, new BasicErrorResponse() { Success = false, ErrorMessage = "No nutritments data found" });
                    }

                    IngridientVMO ingridient = new IngridientVMO()
                    {
                        Name = existingDocument.Product_name,
                        Id = existingDocument.Id,
                        Fats = existingDocument.Nutriments.Fat,
                        Carbs = existingDocument.Nutriments.Carbs,
                        Proteins = existingDocument.Nutriments.Proteins,
                        Prep_For = ConvertToDoubleClean(existingDocument.Nutrition_data_prepared_per),
                        Kcal = existingDocument.Nutriments.EnergyKcal,
                    };

                    return (ingridient, new BasicErrorResponse() { Success = true });
                }

                return (null, new BasicErrorResponse() { Success = false, ErrorMessage = "No occurencies" });
            }
            catch (Exception ex)
            {
                return (null, new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message });
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
                        Prep_For = ConvertToDoubleClean(doc.Nutrition_data_prepared_per),
                        Kcal = doc.Nutriments.EnergyKcal,
                    })
                    .ToList();

                if (ingridients.Count == 0)
                {
                    return (null, new BasicErrorResponse() { Success = false, ErrorMessage = "No results found" });
                }

                return (ingridients, new BasicErrorResponse() { Success = true });
            }
            catch (Exception ex)
            {
                return (null, new BasicErrorResponse() { Success = false, ErrorMessage = ex.Message });
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
                    errorResponse.ErrorMessage = "document not found";
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
                errorResponse.ErrorMessage = ex.Message;
                return (errorResponse, model);
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
                else
                    return new BasicErrorResponse { Success = false, ErrorMessage = "Meal not found or could not be removed" };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse { ErrorMessage = ex.Message, Success = false };
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
                    return new BasicErrorResponse { ErrorMessage = "Meal or ingredient not found.", Success = false };


                var dailyPlan = dietDocument.DailyPlans.FirstOrDefault(dp => dp.Date == model.Date.Date);
                if (dailyPlan == null)
                    return new BasicErrorResponse { ErrorMessage = "Daily plan not found", Success = false };

                var meal = dailyPlan.Meals.FirstOrDefault(m => m.PublicId == model.MealPublicId);
                if (meal == null)
                    return new BasicErrorResponse { ErrorMessage = "Meal not found", Success = false };


                var ingredientToRemove = meal.Ingridient
                    .FirstOrDefault(i => i.publicId == model.IngridientId &&
                                         i.WeightValue == model.WeightValue &&
                                         i.Name == model.IngridientName);

                if (ingredientToRemove == null)
                {
                    return new BasicErrorResponse { ErrorMessage = "Ingridient not found", Success = false };
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
                    return new BasicErrorResponse { ErrorMessage = "Failed to update", Success = false };
                }

                return new BasicErrorResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse { ErrorMessage = ex.Message, Success = false };
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
                    return new BasicErrorResponse { Success = true };
                }
                else
                {
                    return new BasicErrorResponse { Success = false, ErrorMessage = "Meal not found or could not be updated" };
                }
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse { ErrorMessage = ex.Message, Success = false };
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
                    return new BasicErrorResponse { ErrorMessage = "Meal or ingridient not found", Success = false };

                var dailyPlan = dietDocument.DailyPlans.FirstOrDefault(dp => dp.Date == model.Date.Date);
                if (dailyPlan == null)
                    return new BasicErrorResponse { ErrorMessage = "Daily plan not found", Success = false };

                var meal = dailyPlan.Meals.FirstOrDefault(m => m.PublicId == model.MealPublicId);
                if (meal == null)
                    return new BasicErrorResponse { ErrorMessage = "Meal not found", Success = false };

                var ingredientToUpdate = meal.Ingridient
                    .FirstOrDefault(i => i.publicId == model.IngridientId &&
                                         i.WeightValue == model.IngridientWeightOld &&
                                         i.Name == model.IngridientName);

                if (ingredientToUpdate == null)
                    return new BasicErrorResponse { ErrorMessage = "ingridient not found", Success = false };

                ingredientToUpdate.WeightValue = model.IngridientWeightNew;

                var updateResult = await _dietCollection.ReplaceOneAsync(
                    filter,
                    dietDocument
                );

                if (updateResult.MatchedCount == 0)
                    return new BasicErrorResponse { ErrorMessage = "Failed to update ingridient", Success = false };

                return new BasicErrorResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new BasicErrorResponse { ErrorMessage = ex.Message, Success = false };
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

