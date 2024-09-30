using ElGato_API.Data;
using ElGato_API.Interfaces;
using ElGato_API.Models.Requests;
using ElGato_API.VM.Requests;
using ElGato_API.VMO.ErrorResponse;

namespace ElGato_API.Services
{
    public class UserRequestService : IUserRequestService
    {
        private readonly AppDbContext _context;
        public UserRequestService(AppDbContext context) 
        { 
            _context = context;
        }
        public async Task<BasicErrorResponse> RequestAddIngredient(string userId, AddProductRequestVM model)
        {
            try
            {
                AddProductRequest request = new AddProductRequest()
                {
                    ProductBrand = model.ProductBrand,
                    ProductEan13 = model.ProductEan13,
                    ProductName = model.ProductName,
                    EnergyKcal = model.EnergyKcal,
                    Carbs = model.Carbs,
                    Fats = model.Fats,
                    Proteins = model.Proteins,
                    UserId = userId
                };

                _context.AddProductRequest.Add(request);
                await _context.SaveChangesAsync();

                return new BasicErrorResponse() { Success = true };

            }catch(Exception ex)
            {
                return new BasicErrorResponse() { ErrorMessage = $"Request not succesfull. {ex.Message}", Success = false };
            }
        }

        public async Task<BasicErrorResponse> RequestReportIngredient(string userId, IngredientReportRequestVM model)
        {
            try
            {
                ReportedIngredients request = new ReportedIngredients()
                {
                    Cause = model.Cause,
                    IngredientName = model.IngredientName,
                    UserId = userId,
                    IngredientId = model.IngredientId
                };
                _context.ReportedIngredients.Add(request);
                await _context.SaveChangesAsync();

                return new BasicErrorResponse() { Success = true };
            }
            catch (Exception ex) {
                return new BasicErrorResponse() { ErrorMessage = $"Request not succesfull. {ex.Message}", Success = false };
            }
        
        }
    }
}
