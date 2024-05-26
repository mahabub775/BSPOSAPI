using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantCompetitionController
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

			if (returningFunction.Method.Name.Contains("GetApplicantCompetitionsByApplicantId"))
				Messages = ExceptionMessages.ApplicantCompetition_List;

			if (returningFunction.Method.Name.Contains("GetApplicantCompetitionById"))
				Messages = ExceptionMessages.ApplicantCompetition_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantCompetition"))
				Messages = ExceptionMessages.ApplicantCompetition_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantCompetition"))
				Messages = ExceptionMessages.ApplicantCompetition_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantCompetition"))
				Messages = ExceptionMessages.ApplicantCompetition_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}