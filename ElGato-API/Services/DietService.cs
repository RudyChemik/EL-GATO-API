using ElGato_API.Interfaces;
using ElGato_API.VMO.Diet;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Questionary;
using MongoDB.Driver;

namespace ElGato_API.Services
{
    public class DietService : IDietService
    {
        public DietService() { }

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

        public async Task<(IngridientVMO? ingridient, BasicErrorResponse error)> GetIngridientByEan(string ean)
        {
            return (new IngridientVMO(), new BasicErrorResponse() { Success = true, ErrorMessage = "brak" });
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

    }

    public class Makros
    {
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
    }
}

