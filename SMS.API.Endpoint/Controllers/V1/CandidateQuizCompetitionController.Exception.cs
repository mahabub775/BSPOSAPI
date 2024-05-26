using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateQuizCompetitionController
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

			if (returningFunction.Method.Name.Contains("GetCandidateQuizCompetitionsByCandidateId"))
				Messages = ExceptionMessages.CandidateQuizCompetition_List;

			if (returningFunction.Method.Name.Contains("GetCandidateQuizCompetitionById"))
				Messages = ExceptionMessages.CandidateQuizCompetition_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateQuizCompetition"))
				Messages = ExceptionMessages.CandidateQuizCompetition_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateQuizCompetition"))
				Messages = ExceptionMessages.CandidateQuizCompetition_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateQuizCompetition"))
				Messages = ExceptionMessages.CandidateQuizCompetition_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}