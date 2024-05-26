using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class BrigadeController
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

			if (returningFunction.Method.Name.Contains("GetBrigades"))
				Messages = ExceptionMessages.Brigade_List;

			if (returningFunction.Method.Name.Contains("GetDistinctBrigades"))
				Messages = ExceptionMessages.Brigade_List;

			if (returningFunction.Method.Name.Contains("GetBrigadeById"))
				Messages = ExceptionMessages.Brigade_Id;

			if (returningFunction.Method.Name.Contains("InsertBrigade"))
				Messages = ExceptionMessages.Brigade_Insert;

			if (returningFunction.Method.Name.Contains("UpdateBrigade"))
				Messages = ExceptionMessages.Brigade_Update;

			if (returningFunction.Method.Name.Contains("DeleteBrigade"))
				Messages = ExceptionMessages.Brigade_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Brigade_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}