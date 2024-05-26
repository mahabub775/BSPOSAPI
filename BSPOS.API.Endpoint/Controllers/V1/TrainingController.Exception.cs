using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class TrainingController
{
	private delegate Task<IActionResult> ReturningFunction();
	private string Messages = "";

	private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
	{
		try
		{
			return await returningFunction();
		}
		catch (Exception ex)
		{
			_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });

			if (returningFunction.Method.Name.Contains("GetTrainings"))
				Messages = ExceptionMessages.Training_List;

			if (returningFunction.Method.Name.Contains("GetDistinctTrainings"))
				Messages = ExceptionMessages.Training_List;

			if (returningFunction.Method.Name.Contains("GetTrainingById"))
				Messages = ExceptionMessages.Training_Id;

			if (returningFunction.Method.Name.Contains("InsertTraining"))
				Messages = ExceptionMessages.Training_Insert;

			if (returningFunction.Method.Name.Contains("UpdateTraining"))
				Messages = ExceptionMessages.Training_Update;

			if (returningFunction.Method.Name.Contains("DeleteTraining"))
				Messages = ExceptionMessages.Training_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Training_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}