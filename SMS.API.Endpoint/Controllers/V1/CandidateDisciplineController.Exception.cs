using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateDisciplineController
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

			if (returningFunction.Method.Name.Contains("GetCandidateDisciplinesByCandidateId"))
				Messages = ExceptionMessages.CandidateDiscipline_List;

			if (returningFunction.Method.Name.Contains("GetCandidateDisciplineById"))
				Messages = ExceptionMessages.CandidateDiscipline_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateDiscipline"))
				Messages = ExceptionMessages.CandidateDiscipline_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateDiscipline"))
				Messages = ExceptionMessages.CandidateDiscipline_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateDiscipline"))
				Messages = ExceptionMessages.CandidateDiscipline_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}