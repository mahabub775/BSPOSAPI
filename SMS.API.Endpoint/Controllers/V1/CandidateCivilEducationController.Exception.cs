using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateCivilEducationController
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

			if (returningFunction.Method.Name.Contains("GetCandidateCivilEducationsByCandidateId"))
				Messages = ExceptionMessages.CandidateCivilEducation_List;

			if (returningFunction.Method.Name.Contains("GetCandidateCivilEducationById"))
				Messages = ExceptionMessages.CandidateCivilEducation_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateCivilEducation"))
				Messages = ExceptionMessages.CandidateCivilEducation_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateCivilEducation"))
				Messages = ExceptionMessages.CandidateCivilEducation_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateCivilEducation"))
				Messages = ExceptionMessages.CandidateCivilEducation_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}