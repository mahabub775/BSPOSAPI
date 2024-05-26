using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CourseController
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

			if (returningFunction.Method.Name.Contains("GetCourses"))
				Messages = ExceptionMessages.Course_List;

			if (returningFunction.Method.Name.Contains("GetDistinctCourses"))
				Messages = ExceptionMessages.Course_List;

			if (returningFunction.Method.Name.Contains("GetCourseById"))
				Messages = ExceptionMessages.Course_Id;

			if (returningFunction.Method.Name.Contains("InsertCourse"))
				Messages = ExceptionMessages.Course_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCourse"))
				Messages = ExceptionMessages.Course_Update;

			if (returningFunction.Method.Name.Contains("DeleteCourse"))
				Messages = ExceptionMessages.Course_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Course_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}