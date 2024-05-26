using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantCertificationController
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

			if (returningFunction.Method.Name.Contains("GetApplicantCertificationsByApplicantId"))
				Messages = ExceptionMessages.ApplicantCertification_List;

			if (returningFunction.Method.Name.Contains("GetApplicantCertificationById"))
				Messages = ExceptionMessages.ApplicantCertification_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantCertification"))
				Messages = ExceptionMessages.ApplicantCertification_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantCertification"))
				Messages = ExceptionMessages.ApplicantCertification_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantCertification"))
				Messages = ExceptionMessages.ApplicantCertification_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}