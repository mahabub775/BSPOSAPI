using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantAddlQualificationController
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

			if (returningFunction.Method.Name.Contains("GetApplicantAddlQualificationsByApplicantId"))
				Messages = ExceptionMessages.ApplicantAddlQualification_List;

			if (returningFunction.Method.Name.Contains("GetApplicantAddlQualificationById"))
				Messages = ExceptionMessages.ApplicantAddlQualification_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantAddlQualification"))
				Messages = ExceptionMessages.ApplicantAddlQualification_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantAddlQualification"))
				Messages = ExceptionMessages.ApplicantAddlQualification_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantAddlQualification"))
				Messages = ExceptionMessages.ApplicantAddlQualification_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}