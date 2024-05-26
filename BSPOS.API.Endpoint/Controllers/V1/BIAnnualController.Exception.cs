using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class BIAnnualController
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

			if (returningFunction.Method.Name.Contains("GetBIAnnuals"))
				Messages = ExceptionMessages.BIAnnual_List;

			if (returningFunction.Method.Name.Contains("GetDistinctBIAnnuals"))
				Messages = ExceptionMessages.BIAnnual_List;

			if (returningFunction.Method.Name.Contains("GetBIAnnualById"))
				Messages = ExceptionMessages.BIAnnual_Id;

			if (returningFunction.Method.Name.Contains("InsertBIAnnual"))
				Messages = ExceptionMessages.BIAnnual_Insert;

			if (returningFunction.Method.Name.Contains("UpdateBIAnnual"))
				Messages = ExceptionMessages.BIAnnual_Update;

			if (returningFunction.Method.Name.Contains("DeleteBIAnnual"))
				Messages = ExceptionMessages.BIAnnual_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.BIAnnual_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}