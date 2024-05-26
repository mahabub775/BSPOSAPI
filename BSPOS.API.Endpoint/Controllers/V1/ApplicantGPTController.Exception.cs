using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantGPTController
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

			if (returningFunction.Method.Name.Contains("GetApplicantGPTsByApplicantId"))
				Messages = ExceptionMessages.ApplicantGPT_List;

			if (returningFunction.Method.Name.Contains("GetApplicantGPTById"))
				Messages = ExceptionMessages.ApplicantGPT_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantGPT"))
				Messages = ExceptionMessages.ApplicantGPT_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantGPT"))
				Messages = ExceptionMessages.ApplicantGPT_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantGPT"))
				Messages = ExceptionMessages.ApplicantGPT_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}