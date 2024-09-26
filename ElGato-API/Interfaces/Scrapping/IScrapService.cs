using ElGato_API.ModelsMongo.Meal;
using ElGato_API.Services;

namespace ElGato_API.Interfaces.Scrapping
{
    public interface IScrapService
    {
        Task<List<ScrapedLink>> GetLinksToScrapFrom();
        Task<List<ScrapedLinkCons>> GetMealLinks(List<ScrapedLink> listLink);
        Task<List<MealsDocument>> ScrapRestInformations(List<ScrapedLinkCons> links);
        Task SaveScrappedMeals(List<MealsDocument> meals);
    }
}
