using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantIPFTController
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

			if (returningFunction.Method.Name.Contains("GetApplicantIPFTsByApplicantId"))
				Messages = ExceptionMessages.ApplicantIPFT_List;

			if (returningFunction.Method.Name.Contains("GetApplicantIPFTById"))
				Messages = ExceptionMessages.ApplicantIPFT_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantIPFT"))
				Messages = ExceptionMessages.ApplicantIPFT_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantIPFT"))
				Messages = ExceptionMessages.ApplicantIPFT_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantIPFT"))
				Messages = ExceptionMessages.ApplicantIPFT_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}