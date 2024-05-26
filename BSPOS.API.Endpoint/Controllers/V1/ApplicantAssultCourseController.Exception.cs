using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantAssultCourseController
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

			if (returningFunction.Method.Name.Contains("GetApplicantAssultCoursesByApplicantId"))
				Messages = ExceptionMessages.ApplicantAssultCourse_List;

			if (returningFunction.Method.Name.Contains("GetApplicantAssultCourseById"))
				Messages = ExceptionMessages.ApplicantAssultCourse_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantAssultCourse"))
				Messages = ExceptionMessages.ApplicantAssultCourse_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantAssultCourse"))
				Messages = ExceptionMessages.ApplicantAssultCourse_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantAssultCourse"))
				Messages = ExceptionMessages.ApplicantAssultCourse_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}