using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElGato_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DietController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly IDietService _dietService;

        public DietController(IJwtService jwtService, IDietService dietService) 
        { 
            _jwtService = jwtService;
            _dietService = dietService;
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetIngridientByEan() 
        {
            return Ok("ABCD, abcd, aaa");
        }
    }
}
