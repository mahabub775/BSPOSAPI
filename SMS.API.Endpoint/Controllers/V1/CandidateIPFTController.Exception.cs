using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateIPFTController
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

			if (returningFunction.Method.Name.Contains("GetCandidateIPFTsByCandidateId"))
				Messages = ExceptionMessages.CandidateIPFT_List;

			if (returningFunction.Method.Name.Contains("GetCandidateIPFTById"))
				Messages = ExceptionMessages.CandidateIPFT_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateIPFT"))
				Messages = ExceptionMessages.CandidateIPFT_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateIPFT"))
				Messages = ExceptionMessages.CandidateIPFT_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateIPFT"))
				Messages = ExceptionMessages.CandidateIPFT_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}