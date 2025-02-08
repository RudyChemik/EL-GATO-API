using ElGato_API.Data.JWT;
using ElGato_API.Interfaces;
using ElGato_API.Services;
using ElGato_API.VM.Training;
using ElGato_API.VMO.ErrorResponse;
using ElGato_API.VMO.Meals;
using ElGato_API.VMO.Training;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ElGato_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TrainingController : Controller
    {
        private readonly ITrainingService _trainingService;
        private readonly IJwtService _jwtService;
        public TrainingController(ITrainingService trainingService, IJwtService jwtService)
        {
            _trainingService = trainingService;
            _jwtService = jwtService;
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(ExerciseVMO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllExercises()
        {
            try
            {
                var res = await _trainingService.GetAllExercises();
                if (!res.error.Success)
                {
                    return res.error.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res.error),
                        _ => BadRequest(res.error)
                    };
                }

                return Ok(res.data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured {ex.Message}", Success = false });
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(List<LikedExercisesVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLikedExercises()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _trainingService.GetAllLikedExercises(userId);

                if (!res.error.Success)
                {
                    return res.error.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res.error),
                        _ => BadRequest(res.error)
                    };
                }

                return Ok(res.data);

            }catch(Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured {ex.Message}", Success = false });
            }
        }

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(TrainingDayVMO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTrainingDay(DateTime date)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();
                var res = await _trainingService.GetUserTrainingDay(userId, date);
                if (!res.error.Success)
                {
                    return res.error.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res.error.ErrorMessage),
                        ErrorCodes.Internal => StatusCode(500, res.error.ErrorMessage),
                        _ => BadRequest(res)
                    };
                }

                return Ok(res.data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured: {ex.Message}", Success = false });
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddExercisesToTrainingDay([FromBody] AddExerciseToTrainingVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                string userId = _jwtService.GetUserIdClaim();
                model.Name = model.Name.Distinct().ToList();
                var res = await _trainingService.AddExercisesToTrainingDay(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res),
                        ErrorCodes.Internal => StatusCode(500, res),
                        _ => BadRequest(res)
                    };
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured {ex.Message}" , Success = false});
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddExerciseToFavourites([FromBody] LikeExerciseVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                string userId = _jwtService.GetUserIdClaim();

                var res = await _trainingService.LikeExercise(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res),
                        ErrorCodes.AlreadyExists => Conflict(res),
                        ErrorCodes.NotFound => NotFound(res),
                        _ => BadRequest(res)
                    };
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured: {ex.Message}", Success = false });
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> AddSeriesToAnExercise([FromBody] List<AddSeriesToAnExerciseVM> model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                string userId = _jwtService.GetUserIdClaim();

                var tasks = model.Select(model => _trainingService.WriteSeriesForAnExercise(userId, model));

                var res = await Task.WhenAll(tasks);
                var failed = res.Where(r => !r.Success).ToList();

                if (failed.Any())
                {
                    var firstError = failed.First();
                    return firstError.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(firstError),
                        ErrorCodes.Internal => StatusCode(500, firstError),
                        _ => BadRequest(firstError),
                    };
                }

                return Ok();

            }
            catch(Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { Success = false, ErrorMessage = $"An internal server error occured {ex.Message}", ErrorCode = ErrorCodes.Internal });
            }
        }


        [HttpDelete]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveExercisesFromFavourites([FromBody] List<LikeExerciseVM> model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Modal state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                string userId = _jwtService.GetUserIdClaim();

                var res = await _trainingService.RemoveExercisesFromLiked(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res),
                        ErrorCodes.NotFound => NotFound(res),
                        _ => BadRequest(res)
                    };
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured {ex.Message}", Success = false });
            }
        }

    }
}
