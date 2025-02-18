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

        [HttpGet]
        [Authorize(Policy = "user")]
        [ProducesResponseType(typeof(SavedTrainingsVMO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSavedTrainings()
        {
            try
            {
                var userId = _jwtService.GetUserIdClaim();

                var res = await _trainingService.GetSavedTrainings(userId);
                if (!res.error.Success)
                {
                    return res.error.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res.error.ErrorMessage),
                        _ => BadRequest(res)
                    }; 
                }

                return Ok(res.data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { Success = false, ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveTraining([FromBody] SaveTrainingVM model)
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

                var userId = _jwtService.GetUserIdClaim();

                var res = await _trainingService.SaveTraining(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.Internal => StatusCode(500, res.ErrorMessage),
                        ErrorCodes.Failed => StatusCode(500, res.ErrorMessage),
                        _ => BadRequest(res.ErrorMessage)
                    };
                }

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { Success = false, ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured {ex.Message}" });
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

                var writeTasks = model.Select(m => _trainingService.WriteSeriesForAnExercise(userId, m));
                var updateTasks = model.Select(m => _trainingService.UpdateExerciseHistory(userId, m.HistoryUpdate, m.Date));

                var allTasks = writeTasks.Concat(updateTasks);

                var res = await Task.WhenAll(allTasks);
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

        [HttpPatch]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateExerciseLikedStatus(string exerciseName)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _trainingService.UpdateExerciseLikedStatus(userId, exerciseName);
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
                return StatusCode(500, new BasicErrorResponse() { Success = false, ErrorMessage = $"An internal server error occured {ex.Message}", ErrorCode = ErrorCodes.Internal });
            }
        }

        [HttpPatch]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateExerciseSeries([FromBody] List<UpdateExerciseSeriesVM> model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return StatusCode(400, new BasicErrorResponse()
                    {
                        Success = false,
                        ErrorMessage = "Model state not valid",
                        ErrorCode = ErrorCodes.ModelStateNotValid,
                    });
                }

                string userId = _jwtService.GetUserIdClaim();
                var patchTasks = model.Select(m => _trainingService.UpdateExerciseSeries(userId, m));
                var patchHistory = model.Select(m => _trainingService.UpdateExerciseHistory(userId, m.HistoryUpdate, m.Date));

                var allTasks = patchTasks.Concat(patchHistory);
                var res = await Task.WhenAll(allTasks);


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
            catch (Exception ex) 
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
        public async Task<IActionResult> RemoveSeriesFromAnExercise([FromBody] List<RemoveSeriesFromExerciseVM> model)
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

                var deleteTasks = model.Select(m => _trainingService.RemoveSeriesFromAnExercise(userId, m));
                var patchTasks = model.Select(m => _trainingService.UpdateExerciseHistory(userId, m.HistoryUpdate, m.Date));

                var allTasks = deleteTasks.Concat(patchTasks);
                var res = await Task.WhenAll(allTasks);

                var failed = res.Where(r => !r.Success).ToList();

                if (failed.Any())
                {
                    var firstError = failed.First();
                    return firstError.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(firstError),
                        ErrorCodes.Failed => StatusCode(500, firstError),
                        ErrorCodes.Internal => StatusCode(500, firstError),
                        _ => BadRequest(firstError),
                    };
                }

                return Ok();

            }
            catch(Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, ErrorMessage = $"An internal server error occured {ex.Message}", Success = false });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveExercisesFromTrainingDay([FromBody] List<RemoveExerciseFromTrainingDayVM> model)
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

                var removeTasks = model.Select(m => _trainingService.RemoveExerciseFromTrainingDay(userId, m));
                var res = await Task.WhenAll(removeTasks);

                var failed = res.Where(r => !r.Success).ToList();
                if (failed.Any())
                {
                    var firstError = failed.First();
                    return firstError.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(firstError),
                        ErrorCodes.Failed => StatusCode(500, firstError),
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

        [HttpDelete]
        [Authorize(Policy = "user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicErrorResponse), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> RemoveTrainingsFromSaved([FromBody] RemoveSavedTrainingsVM model)
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

                var userId = _jwtService.GetUserIdClaim();

                var res = await _trainingService.RemoveTrainingsFromSaved(userId, model);
                if (!res.Success)
                {
                    return res.ErrorCode switch
                    {
                        ErrorCodes.NotFound => NotFound(res.ErrorMessage),
                        ErrorCodes.Internal => StatusCode(500, res.ErrorMessage),
                        _ => BadRequest(res.ErrorMessage),
                    };
                }

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, new BasicErrorResponse() { ErrorCode = ErrorCodes.Internal, Success = false, ErrorMessage = $"An internal server error occured {ex.Message}" });             
            }
        }

    }
}
