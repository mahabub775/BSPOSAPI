using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateCertificationController
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

			if (returningFunction.Method.Name.Contains("GetCandidateCertificationsByCandidateId"))
				Messages = ExceptionMessages.CandidateCertification_List;

			if (returningFunction.Method.Name.Contains("GetCandidateCertificationById"))
				Messages = ExceptionMessages.CandidateCertification_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateCertification"))
				Messages = ExceptionMessages.CandidateCertification_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateCertification"))
				Messages = ExceptionMessages.CandidateCertification_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateCertification"))
				Messages = ExceptionMessages.CandidateCertification_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}