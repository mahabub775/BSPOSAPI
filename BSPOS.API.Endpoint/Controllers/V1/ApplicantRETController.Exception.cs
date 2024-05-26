using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantRETController
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

			if (returningFunction.Method.Name.Contains("GetApplicantRETsByApplicantId"))
				Messages = ExceptionMessages.ApplicantRET_List;

			if (returningFunction.Method.Name.Contains("GetApplicantRETById"))
				Messages = ExceptionMessages.ApplicantRET_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantRET"))
				Messages = ExceptionMessages.ApplicantRET_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantRET"))
				Messages = ExceptionMessages.ApplicantRET_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantRET"))
				Messages = ExceptionMessages.ApplicantRET_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}