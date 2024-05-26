using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateGPTController
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

			if (returningFunction.Method.Name.Contains("GetCandidateGPTsByCandidateId"))
				Messages = ExceptionMessages.CandidateGPT_List;

			if (returningFunction.Method.Name.Contains("GetCandidateGPTById"))
				Messages = ExceptionMessages.CandidateGPT_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateGPT"))
				Messages = ExceptionMessages.CandidateGPT_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateGPT"))
				Messages = ExceptionMessages.CandidateGPT_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateGPT"))
				Messages = ExceptionMessages.CandidateGPT_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}