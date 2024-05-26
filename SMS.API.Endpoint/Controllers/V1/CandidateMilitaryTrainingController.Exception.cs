using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateMilitaryTrainingController
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

			if (returningFunction.Method.Name.Contains("GetCandidateMilitaryTrainingsByCandidateId"))
				Messages = ExceptionMessages.CandidateMilitaryTraining_List;

			if (returningFunction.Method.Name.Contains("GetCandidateMilitaryTrainingById"))
				Messages = ExceptionMessages.CandidateMilitaryTraining_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateMilitaryTraining"))
				Messages = ExceptionMessages.CandidateMilitaryTraining_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateMilitaryTraining"))
				Messages = ExceptionMessages.CandidateMilitaryTraining_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateMilitaryTraining"))
				Messages = ExceptionMessages.CandidateMilitaryTraining_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}