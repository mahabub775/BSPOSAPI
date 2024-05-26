using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class UnitController
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

			if (returningFunction.Method.Name.Contains("GetUnits"))
				Messages = ExceptionMessages.Unit_List;

			if (returningFunction.Method.Name.Contains("GetDistinctUnits"))
				Messages = ExceptionMessages.Unit_List;

			if (returningFunction.Method.Name.Contains("GetUnitById"))
				Messages = ExceptionMessages.Unit_Id;

			if (returningFunction.Method.Name.Contains("InsertUnit"))
				Messages = ExceptionMessages.Unit_Insert;

			if (returningFunction.Method.Name.Contains("UpdateUnit"))
				Messages = ExceptionMessages.Unit_Update;

			if (returningFunction.Method.Name.Contains("DeleteUnit"))
				Messages = ExceptionMessages.Unit_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Unit_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}