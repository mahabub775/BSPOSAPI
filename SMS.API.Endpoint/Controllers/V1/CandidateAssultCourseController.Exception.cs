using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateAssultCourseController
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

			if (returningFunction.Method.Name.Contains("GetCandidateAssultCoursesByCandidateId"))
				Messages = ExceptionMessages.CandidateAssultCourse_List;

			if (returningFunction.Method.Name.Contains("GetCandidateAssultCourseById"))
				Messages = ExceptionMessages.CandidateAssultCourse_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateAssultCourse"))
				Messages = ExceptionMessages.CandidateAssultCourse_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateAssultCourse"))
				Messages = ExceptionMessages.CandidateAssultCourse_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateAssultCourse"))
				Messages = ExceptionMessages.CandidateAssultCourse_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}