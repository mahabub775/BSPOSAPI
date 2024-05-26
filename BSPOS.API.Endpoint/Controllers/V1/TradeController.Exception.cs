using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class TradeController
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

			if (returningFunction.Method.Name.Contains("GetTrades"))
				Messages = ExceptionMessages.Trade_List;

			if (returningFunction.Method.Name.Contains("GetDistinctTrades"))
				Messages = ExceptionMessages.Trade_List;

			if (returningFunction.Method.Name.Contains("GetTradeById"))
				Messages = ExceptionMessages.Trade_Id;

			if (returningFunction.Method.Name.Contains("InsertTrade"))
				Messages = ExceptionMessages.Trade_Insert;

			if (returningFunction.Method.Name.Contains("UpdateTrade"))
				Messages = ExceptionMessages.Trade_Update;

			if (returningFunction.Method.Name.Contains("DeleteTrade"))
				Messages = ExceptionMessages.Trade_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Trade_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}