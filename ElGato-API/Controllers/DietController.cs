﻿using Amazon.Runtime.Internal;
using Azure;
using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.ModelsMongo.Diet;
using ElGato_API.VM.Diet;
using ElGato_API.VMO.Diet;
using ElGato_API.VMO.ErrorResponse;
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

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddNewMeal([FromBody] AddNewMealVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.AddNewMeal(userId, model.MealName, model.Date);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.AlreadyExists => Conflict(res),
                        ErrorCodes.Internal => StatusCode(500, res),
                        _ => BadRequest(res),
                    };
                }

                return Ok(new BasicErrorResponse
                {
                    Success = true,
                    ErrorMessage = "Meal added successfully"
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }


        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddIngridientToMeal([FromBody] AddIngridientVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.AddIngridientToMeal(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Internal => StatusCode(500, res),
                        _ => BadRequest(res),
                    };
                }

                return Ok(new BasicErrorResponse
                {
                    Success = true,
                    ErrorMessage = "Ingridient added to meal sucesfully."
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddIngriedientsToMeal([FromBody] AddIngridientsVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.AddIngredientsToMeals(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Internal => StatusCode(500, res),
                        _ => BadRequest(res),
                    };
                }                 

                return Ok(new BasicErrorResponse()
                {
                    Success = true,
                    ErrorMessage = "Ingridient added sucesfully.",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddWater([FromBody] AddWaterVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.AddWater(userId, model.Water, model.Date);
                if (!res.Success) 
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Internal => StatusCode(500, res),
                        _ => BadRequest(res),
                    };
                }
                    

                return Ok(new BasicErrorResponse()
                {
                    Success = true,
                    ErrorMessage = $"{model.Water} ml of water added sucesfully"
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddMealToSavedMeals([FromBody]SaveIngridientMealVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.AddMealToSavedMeals(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.AlreadyExists => Conflict(res),
                        ErrorCodes.Internal => StatusCode(500, res),
                        _ => BadRequest(res),
                    };
                }

                return Ok(new BasicErrorResponse()
                {
                    Success = true,
                    ErrorMessage = $"Meal added sucesfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddMealFromSaved([FromBody] AddMealFromSavedVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();
                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.AddMealFromSavedMeals(userId, model);

                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Internal => StatusCode(500, res),
                        _ => BadRequest(res),
                    };
                }


                return Ok(new BasicErrorResponse()
                {
                    ErrorMessage = "Meal added succesfully.",
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(DietDocVMO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserDietDoc()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _dietService.GetUserDoc(userId);
                if (!res.errorResponse.Success)
                {
                    return res.errorResponse.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res.errorResponse),
                        ErrorCodes.NotFound => NotFound(res.errorResponse),
                        _ => BadRequest(res),
                    };
                }
                    

                return Ok(res.model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(DietDayVMO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserDietDay(DateTime date)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _dietService.GetUserDietDay(userId, date);
                if (!res.errorResponse.Success)
                {
                    return res.errorResponse.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res.errorResponse),
                        ErrorCodes.Internal => StatusCode(500, res.errorResponse),
                        _ => BadRequest(res.errorResponse),
                    };
                }
                    

                return Ok(res.model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(IngridientVMO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIngridientByEan(string ean)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _dietService.GetIngridientByEan(ean);
                if (res.ingridient == null)
                {
                    return res.error.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res.error),
                        ErrorCodes.Internal => StatusCode(500, res.error),
                        _ => BadRequest(res.error),
                    };
                }

                return Ok(res.ingridient);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(List<IngridientVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetListOfCorrelatedItemByName(string name)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _dietService.GetListOfIngridientsByName(name);

                if (!res.error.Success)
                {
                    return res.error.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res.error),
                        ErrorCodes.Internal => StatusCode(500 ,res.error),
                        _ => BadRequest(res.error)
                    };
                }
                    
                return Ok(res.ingridients);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(List<MealPlan>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSavedMeals()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _dietService.GetSavedMeals(userId);
                if (!res.errorResponse.Success)
                {
                    return res.errorResponse.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res.errorResponse),
                        _ => BadRequest(res.errorResponse)
                    };
                }

                return Ok(res.model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMeal([FromBody]DeleteMealVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                
               var res = await _dietService.DeleteMeal(userId, model.PublicId, model.Date);
               if (!res.Success)
               {
                   return res.ErrorCode switch
                   {
                      ErrorCodes.NotFound => NotFound(res),
                      ErrorCodes.Internal => StatusCode(500, res),
                      _ => BadRequest(res),
                   };
               }
                        
               return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveIngridientFromMeal([FromBody] RemoveIngridientVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.DeleteIngridientFromMeal(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Failed => BadRequest(res),
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Internal => StatusCode(500, res),
                        _ => BadRequest(res),
                    };
                }
                    
                return Ok(new BasicErrorResponse()
                {
                    ErrorMessage = "Ingridient deleted sucesfully",
                    Success = true                   
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveMealFromSavedMeals(string mealName)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _dietService.RemoveMealFromSaved(userId, mealName);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res),
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Failed => BadRequest(res),
                        _ => BadRequest(res)
                    };
                }

                return Ok(new BasicErrorResponse()
                {
                    Success = true,
                    ErrorMessage = "Meal removed sucesfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMealsFromSaved([FromBody] DeleteSavedMealsVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.DeleteMealsFromSaved(userId, model);
                if (!res.Success) 
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res),
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Failed => BadRequest(res),
                        _ => BadRequest(res)
                    };
                };

                return Ok(new BasicErrorResponse()
                {
                    ErrorMessage = "Meals deleted from saved sucesfully",
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpPatch]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateIngridientWeightValue([FromBody] UpdateIngridientVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.UpdateIngridientWeightValue(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res),
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Failed => BadRequest(res),
                        _ => BadRequest(res)
                    };
                }
                    
                return Ok(new BasicErrorResponse()
                {
                    ErrorMessage = "Sucesfully updated ingridient weight value.",
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpPatch]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMealName([FromBody] UpdateMealNameVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.UpdateMealName(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res),
                        ErrorCodes.NotFound => NotFound(res),
                        _ => BadRequest(res)
                    };
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

        [HttpPatch]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSavedMealIngridientWeight([FromBody] UpdateSavedMealWeightVM model)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                var res = await _dietService.UpdateSavedMealIngridientWeight(userId, model);
                if (!res.Success) 
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res),
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Failed => BadRequest(res),
                        _ => BadRequest(res)
                    };
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An Internal server error occured {ex.Message}", Success = false, });
            }
        }

    }
}
