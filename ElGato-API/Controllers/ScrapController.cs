using ElGato_API.Interfaces.Scrapping;
using Microsoft.AspNetCore.Mvc;

namespace ElGato_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ScrapController : Controller
    {
        private readonly IScrapService _scrapService;

        public ScrapController(IScrapService scrapService)
        {
            _scrapService = scrapService;
        }

        [HttpGet]
        public async Task ScrapRecepies()
        {
            var res = await _scrapService.GetLinksToScrapFrom();

            var correctLinks = await _scrapService.GetMealLinks(res);

            var finalRes = await _scrapService.ScrapRestInformations(correctLinks);

            await _scrapService.SaveScrappedMeals(finalRes);
        }

    }
}
