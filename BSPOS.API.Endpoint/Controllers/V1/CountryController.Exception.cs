using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CountryController
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

			if (returningFunction.Method.Name.Contains("GetCountrys"))
				Messages = ExceptionMessages.Country_List;

			if (returningFunction.Method.Name.Contains("GetDistinctCountrys"))
				Messages = ExceptionMessages.Country_List;

			if (returningFunction.Method.Name.Contains("GetCountryById"))
				Messages = ExceptionMessages.Country_Id;

			if (returningFunction.Method.Name.Contains("InsertCountry"))
				Messages = ExceptionMessages.Country_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCountry"))
				Messages = ExceptionMessages.Country_Update;

			if (returningFunction.Method.Name.Contains("DeleteCountry"))
				Messages = ExceptionMessages.Country_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Country_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}