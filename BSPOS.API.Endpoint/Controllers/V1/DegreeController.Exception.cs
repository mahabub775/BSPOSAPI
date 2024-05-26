using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class DegreeController
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

			if (returningFunction.Method.Name.Contains("GetDegrees"))
				Messages = ExceptionMessages.Degree_List;

			if (returningFunction.Method.Name.Contains("GetDistinctDegrees"))
				Messages = ExceptionMessages.Degree_List;

			if (returningFunction.Method.Name.Contains("GetDegreeById"))
				Messages = ExceptionMessages.Degree_Id;

			if (returningFunction.Method.Name.Contains("InsertDegree"))
				Messages = ExceptionMessages.Degree_Insert;

			if (returningFunction.Method.Name.Contains("UpdateDegree"))
				Messages = ExceptionMessages.Degree_Update;

			if (returningFunction.Method.Name.Contains("DeleteDegree"))
				Messages = ExceptionMessages.Degree_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Degree_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}