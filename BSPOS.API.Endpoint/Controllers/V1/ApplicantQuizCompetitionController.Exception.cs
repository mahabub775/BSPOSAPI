using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantQuizCompetitionController
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

			if (returningFunction.Method.Name.Contains("GetApplicantQuizCompetitionsByApplicantId"))
				Messages = ExceptionMessages.ApplicantQuizCompetition_List;

			if (returningFunction.Method.Name.Contains("GetApplicantQuizCompetitionById"))
				Messages = ExceptionMessages.ApplicantQuizCompetition_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantQuizCompetition"))
				Messages = ExceptionMessages.ApplicantQuizCompetition_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantQuizCompetition"))
				Messages = ExceptionMessages.ApplicantQuizCompetition_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantQuizCompetition"))
				Messages = ExceptionMessages.ApplicantQuizCompetition_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}