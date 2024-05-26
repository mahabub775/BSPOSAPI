using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantCivilEducationController
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

			if (returningFunction.Method.Name.Contains("GetApplicantCivilEducationsByApplicantId"))
				Messages = ExceptionMessages.ApplicantCivilEducation_List;

			if (returningFunction.Method.Name.Contains("GetApplicantCivilEducationById"))
				Messages = ExceptionMessages.ApplicantCivilEducation_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantCivilEducation"))
				Messages = ExceptionMessages.ApplicantCivilEducation_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantCivilEducation"))
				Messages = ExceptionMessages.ApplicantCivilEducation_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantCivilEducation"))
				Messages = ExceptionMessages.ApplicantCivilEducation_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}