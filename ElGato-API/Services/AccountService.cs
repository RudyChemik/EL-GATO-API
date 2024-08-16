using ElGato_API.Data;
using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.Models.User;
using ElGato_API.VM;
using ElGato_API.VM.User_Auth;
using ElGato_API.VMO.Diet;
using ElGato_API.VMO.UserAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ElGato_API.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        public AccountService(AppDbContext context, UserManager<AppUser> userManager, IConfiguration configuration, IJwtService jwtService)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        public async Task<bool> IsEmailAlreadyUsed(string email)
        {
            var res = await _context.AppUser.FirstOrDefaultAsync(a=>a.Email == email);

            if (res != null)
                return true;

            return false;
        }

        public async Task<LoginVMO> LoginUser(LoginVM loginVM)
        {
            var user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user == null)
                return CreateFailedLoginResult("E-mail address is invalid");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginVM.Password);

            if (!isPasswordValid)
                return CreateFailedLoginResult("Password is invalid");

            var userRoles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateJwtToken(user, loginVM.Email, userRoles);

            return new LoginVMO
            {
                IdentityResult = IdentityResult.Success,
                JwtToken = token
            };
        }       

        public async Task<IdentityResult> RegisterUser(RegisterWithQuestVM model, CalorieIntakeVMO calorieIntake)
        {
            var userInformation = new UserInformation
            {
                Age = model.Questionary.Age,
                Weight = model.Questionary.Weight,
                Height = model.Questionary.Height,
                TrainingDays = model.Questionary.TrainingDays,
                Sleep = model.Questionary.Sleep,
                DailyTimeSpendWorking = model.Questionary.DailyTimeSpendWorking,
                JobType = model.Questionary.JobType,
                Woman = model.Questionary.Woman,
                BodyType = model.Questionary.BodyType,
                Goal = model.Questionary.Goal,               
            };

            var calories = new CalorieInformation
            {
                Carbs = calorieIntake.Carbs,
                Fat = calorieIntake.Fat,
                Kcal = calorieIntake.Kcal,
                Protein = calorieIntake.Protein,
                Kj = calorieIntake.Kj,
            };

            var user = new AppUser { UserName = model.Email, Email = model.Email, Name = model.Questionary.Name, UserInformation = userInformation, Metric = model.Questionary.Metric, CalorieInformation = calories };

            var res = await _userManager.CreateAsync(user, model.Password);

            if (res.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
            }

            return res;
        }


        //privets
        private LoginVMO CreateFailedLoginResult(string errorMessage)
        {
            return new LoginVMO
            {
                IdentityResult = IdentityResult.Failed(new IdentityError { Description = errorMessage })
            };
        }

    }
}
