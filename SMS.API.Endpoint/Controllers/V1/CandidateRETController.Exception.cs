using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateRETController
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

			if (returningFunction.Method.Name.Contains("GetCandidateRETsByCandidateId"))
				Messages = ExceptionMessages.CandidateRET_List;

			if (returningFunction.Method.Name.Contains("GetCandidateRETById"))
				Messages = ExceptionMessages.CandidateRET_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateRET"))
				Messages = ExceptionMessages.CandidateRET_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateRET"))
				Messages = ExceptionMessages.CandidateRET_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateRET"))
				Messages = ExceptionMessages.CandidateRET_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}