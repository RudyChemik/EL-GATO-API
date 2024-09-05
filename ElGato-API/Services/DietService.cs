﻿using ElGato_API.Interfaces;
using ElGato_API.ModelsMongo.Diet;
using ElGato_API.ModelsMongo.Diet.History;
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

        
    }

    public class Makros
    {
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
    }
}

