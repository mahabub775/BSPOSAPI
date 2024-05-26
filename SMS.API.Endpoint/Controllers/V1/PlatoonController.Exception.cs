using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class PlatoonController
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

			if (returningFunction.Method.Name.Contains("GetPlatoons"))
				Messages = ExceptionMessages.Platoon_List;

			if (returningFunction.Method.Name.Contains("GetDistinctPlatoons"))
				Messages = ExceptionMessages.Platoon_List;

			if (returningFunction.Method.Name.Contains("GetPlatoonById"))
				Messages = ExceptionMessages.Platoon_Id;

			if (returningFunction.Method.Name.Contains("InsertPlatoon"))
				Messages = ExceptionMessages.Platoon_Insert;

			if (returningFunction.Method.Name.Contains("UpdatePlatoon"))
				Messages = ExceptionMessages.Platoon_Update;

			if (returningFunction.Method.Name.Contains("DeletePlatoon"))
				Messages = ExceptionMessages.Platoon_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Platoon_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}