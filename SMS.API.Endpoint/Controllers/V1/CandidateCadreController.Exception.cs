using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateCadreController
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

			if (returningFunction.Method.Name.Contains("GetCandidateCadresByCandidateId"))
				Messages = ExceptionMessages.CandidateCadre_List;

			if (returningFunction.Method.Name.Contains("GetCandidateCadreById"))
				Messages = ExceptionMessages.CandidateCadre_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateCadre"))
				Messages = ExceptionMessages.CandidateCadre_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateCadre"))
				Messages = ExceptionMessages.CandidateCadre_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateCadre"))
				Messages = ExceptionMessages.CandidateCadre_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}