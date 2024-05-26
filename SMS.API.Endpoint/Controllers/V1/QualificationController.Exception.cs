using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class QualificationController
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

			if (returningFunction.Method.Name.Contains("GetQualifications"))
				Messages = ExceptionMessages.Qualification_List;

			if (returningFunction.Method.Name.Contains("GetDistinctQualifications"))
				Messages = ExceptionMessages.Qualification_List;

			if (returningFunction.Method.Name.Contains("GetQualificationById"))
				Messages = ExceptionMessages.Qualification_Id;

			if (returningFunction.Method.Name.Contains("InsertQualification"))
				Messages = ExceptionMessages.Qualification_Insert;

			if (returningFunction.Method.Name.Contains("UpdateQualification"))
				Messages = ExceptionMessages.Qualification_Update;

			if (returningFunction.Method.Name.Contains("DeleteQualification"))
				Messages = ExceptionMessages.Qualification_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Qualification_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}