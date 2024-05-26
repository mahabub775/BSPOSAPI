using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class BAASectionController
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

			if (returningFunction.Method.Name.Contains("GetBAASections"))
				Messages = ExceptionMessages.BAASection_List;

			if (returningFunction.Method.Name.Contains("GetDistinctBAASections"))
				Messages = ExceptionMessages.BAASection_List;

			if (returningFunction.Method.Name.Contains("GetBAASectionById"))
				Messages = ExceptionMessages.BAASection_Id;

			if (returningFunction.Method.Name.Contains("InsertBAASection"))
				Messages = ExceptionMessages.BAASection_Insert;

			if (returningFunction.Method.Name.Contains("UpdateBAASection"))
				Messages = ExceptionMessages.BAASection_Update;

			if (returningFunction.Method.Name.Contains("DeleteBAASection"))
				Messages = ExceptionMessages.BAASection_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.BAASection_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}