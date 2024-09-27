using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using ElGato_API.Interfaces.Scrapping;
using ElGato_API.ModelsMongo.Meal;
using HtmlAgilityPack;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ElGato_API.Services
{
    public class ScrapService : IScrapService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly IMongoCollection<MealsDocument> _mealCollection;

        public ScrapService(IMongoDatabase mongo)
        {
            _mealCollection = mongo.GetCollection<MealsDocument>("MealsDoc");
        }

        public async Task<List<ScrapedLink>> GetLinksToScrapFrom()
        {
            var url = "https://www.allrecipes.com/recipes-a-z-6735880";
            var scrapedLinks = new List<ScrapedLink>();

            try
            {
                var response = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                var alphabeticalLists = doc.DocumentNode.SelectNodes("//div[contains(@class, 'mntl-alphabetical-list')]");

                foreach (var list in alphabeticalLists)
                {
                    var items = list.SelectNodes(".//li[contains(@class, 'mntl-link-list__item')]");

                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var link = item.SelectSingleNode(".//a");
                            if (link != null)
                            {
                                var scrapedLink = new ScrapedLink
                                {
                                    Name = link.InnerText.Trim(),
                                    Url = link.GetAttributeValue("href", string.Empty)
                                };
                                scrapedLinks.Add(scrapedLink);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return scrapedLinks;
        }

        public async Task<List<ScrapedLinkCons>> GetMealLinks(List<ScrapedLink> listLink)
        {
            var scrapedLinks = new List<ScrapedLinkCons>();

            foreach (var linkInfo in listLink)
            {
                string linkMain = linkInfo.Url;

                try
                {
                    var response = await client.GetStringAsync(linkMain);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(response);

                    var linkNodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'comp mntl-card-list-items mntl-document-card mntl-card card card--no-image')]");

                    if (linkNodes != null)
                    {
                        foreach (var node in linkNodes)
                        {
                            var titleNode = node.SelectSingleNode(".//span[@class='card__title-text ']");
                            var name = titleNode != null ? titleNode.InnerText.Trim() : "No Title Found";
                            var url = node.GetAttributeValue("href", string.Empty);

                            if (!string.IsNullOrEmpty(url))
                            {
                                scrapedLinks.Add(new ScrapedLinkCons
                                {
                                    Name = name,
                                    Url = url,
                                    Category = linkInfo.Name,
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return scrapedLinks;
        }


        public async Task<List<MealsDocument>> ScrapRestInformations(List<ScrapedLinkCons> links)
        {
            List<MealsDocument> mealList = new List<MealsDocument>();           

            foreach (var link in links)
            {
                string linkMain = link.Url;

                try
                {
                    List<string> listCategory = new List<string>();
                    listCategory.Add(link.Category);

                    var response = await client.GetStringAsync(linkMain);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(response);

                    var breadcrumbNodes = doc.DocumentNode.SelectNodes("//ul[@id='mntl-universal-breadcrumbs_1-0']//li//span[@class='link__wrapper']");
                    if (breadcrumbNodes != null)
                    {
                        foreach (var node in breadcrumbNodes)
                        {
                            listCategory.Add(node.InnerText.Trim());
                        }
                    }

                    var descriptionNode = doc.DocumentNode.SelectSingleNode("//p[contains(@class, 'article-subheading type--dog')]");
                    var description = descriptionNode?.InnerHtml.Trim();

                    var totalTimeNode = doc.DocumentNode.SelectSingleNode("//div[@class='loc article-content']//div[@class='comp article-content mntl-block']//div[@class='comp mm-recipes-details']//div[@class='mm-recipes-details__content']//div[@class='mm-recipes-details__item']//div[@class='mm-recipes-details__label' and text()='Total Time:']/following-sibling::div[@class='mm-recipes-details__value']");
                    var totalTime = totalTimeNode?.InnerText.Trim();

                    var ingredientNodes = doc.DocumentNode.SelectNodes("//div[@id='mm-recipes-lrs-ingredients_1-0']//ul[@class='mm-recipes-structured-ingredients__list']/li"); List<string> ingredients = new List<string>();
                    if (ingredientNodes != null)
                    {
                        foreach (var node in ingredientNodes)
                        {
                            var quantity = node.SelectSingleNode(".//span[@data-ingredient-quantity='true']")?.InnerText.Trim();
                            var unit = node.SelectSingleNode(".//span[@data-ingredient-unit='true']")?.InnerText.Trim();
                            var name = node.SelectSingleNode(".//span[@data-ingredient-name='true']")?.InnerText.Trim();

                            string ingredient = $"{quantity} {unit} {name}".Trim();
                            ingredients.Add(ingredient);
                        }
                    }

                    var stepNodes = doc.DocumentNode.SelectNodes("//ol[contains(@class, 'mntl-sc-block-group--OL')]/li/p");
                    List<string> steps = new List<string>();
                    if (stepNodes != null)
                    {
                        foreach (var node in stepNodes)
                        {
                            string step = node.InnerText.Trim();
                            steps.Add(step);
                        }
                    }

                    var imgNode = doc.DocumentNode.SelectSingleNode("//div[@class='loc article-content']//div[@class='primary-image__media']//div[@class='img-placeholder']//img");
                    var imgUrl = imgNode?.GetAttributeValue("src", null);
                    string path = "";

                    if (!string.IsNullOrEmpty(imgUrl))
                    {
                        var imageBytes = await client.GetByteArrayAsync(imgUrl);
                        Random rnd = new Random();

                        string fileName = $"{Guid.NewGuid()}{rnd.Next(1,1000000)}.jpg";
                        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Images/MealImages", fileName);

                        await File.WriteAllBytesAsync(imagePath, imageBytes);
                        path = $"/meal-images/{fileName}";
                    }

                    var servingsNode = doc.DocumentNode.SelectSingleNode("//div[@class='loc article-content']//div[@class='comp article-content mntl-block']//div[@class='comp mm-recipes-details']//div[@class='mm-recipes-details__content']//div[@class='mm-recipes-details__item']//div[@class='mm-recipes-details__label' and text()='Servings:']/following-sibling::div[@class='mm-recipes-details__value']");
                    var servings = servingsNode?.InnerText.Trim();

                    var caloriesNode = doc.DocumentNode.SelectSingleNode("//div[@class='comp mm-recipes-nutrition-facts mntl-block']//div[@class='comp mm-recipes-nutrition-facts-summary']//table[@class='mm-recipes-nutrition-facts-summary__table']//tbody[@class='mm-recipes-nutrition-facts-summary__table-body']//tr[td[contains(text(),'Calories')]]/td[@class='mm-recipes-nutrition-facts-summary__table-cell type--dog-bold']");
                    var fatNode = doc.DocumentNode.SelectSingleNode("//div[@class='comp mm-recipes-nutrition-facts mntl-block']//div[@class='comp mm-recipes-nutrition-facts-summary']//table[@class='mm-recipes-nutrition-facts-summary__table']//tbody[@class='mm-recipes-nutrition-facts-summary__table-body']//tr[td[contains(text(),'Fat')]]/td[@class='mm-recipes-nutrition-facts-summary__table-cell type--dog-bold']");
                    var carbsNode = doc.DocumentNode.SelectSingleNode("//div[@class='comp mm-recipes-nutrition-facts mntl-block']//div[@class='comp mm-recipes-nutrition-facts-summary']//table[@class='mm-recipes-nutrition-facts-summary__table']//tbody[@class='mm-recipes-nutrition-facts-summary__table-body']//tr[td[contains(text(),'Carbs')]]/td[@class='mm-recipes-nutrition-facts-summary__table-cell type--dog-bold']");
                    var proteinNode = doc.DocumentNode.SelectSingleNode("//div[@class='comp mm-recipes-nutrition-facts mntl-block']//div[@class='comp mm-recipes-nutrition-facts-summary']//table[@class='mm-recipes-nutrition-facts-summary__table']//tbody[@class='mm-recipes-nutrition-facts-summary__table-body']//tr[td[contains(text(),'Protein')]]/td[@class='mm-recipes-nutrition-facts-summary__table-cell type--dog-bold']");



                    var calories = caloriesNode?.InnerText.Trim();
                    var fat = fatNode?.InnerText.Trim();
                    var carbs = carbsNode?.InnerText.Trim();
                    var protein = proteinNode?.InnerText.Trim();


                    if (protein == null || carbs == null || fat == null || calories == null || servings == null)
                    {

                    }
                    else
                    {
                        MealsMakro makro = new MealsMakro()
                        {
                            Kcal = Convert.ToDouble(calories.Replace("g", "").Trim()),
                            Carbs = Convert.ToDouble(carbs.Replace("g", "").Trim()),
                            Fats = Convert.ToDouble(fat.Replace("g", "").Trim()),
                            Protein = Convert.ToDouble(protein.Replace("g", "").Trim()),
                            PreperedPer = 0,
                            Servings = Convert.ToDouble(servings),
                        };

                        MealsDocument meal = new MealsDocument()
                        {
                            UserId = "356df418-75f7-41f3-b59d-6b534e270e21",
                            Name = link.Name,
                            Time = totalTime,
                            Description = description,
                            Img = path,
                            MealsMakro = makro,
                            Link = link.Url,
                            Ingridients = ingredients,
                            IngridientsIds = new List<string>(),
                            Steps = steps,
                            Categories = listCategory

                        };
                        mealList.Add(meal);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return mealList;
        }
        public async Task SaveScrappedMeals(List<MealsDocument> meals)
        {
            try
            {
                await _mealCollection.InsertManyAsync(meals);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }

    public class ScrapedLink
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class ScrapedLinkCons
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
    }

}
