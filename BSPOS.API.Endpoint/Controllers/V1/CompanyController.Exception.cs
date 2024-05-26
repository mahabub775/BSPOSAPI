using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CompanyController
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

			if (returningFunction.Method.Name.Contains("GetCompanys"))
				Messages = ExceptionMessages.Company_List;

			if (returningFunction.Method.Name.Contains("GetDistinctCompanys"))
				Messages = ExceptionMessages.Company_List;

			if (returningFunction.Method.Name.Contains("GetCompanyById"))
				Messages = ExceptionMessages.Company_Id;

			if (returningFunction.Method.Name.Contains("InsertCompany"))
				Messages = ExceptionMessages.Company_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCompany"))
				Messages = ExceptionMessages.Company_Update;

			if (returningFunction.Method.Name.Contains("DeleteCompany"))
				Messages = ExceptionMessages.Company_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Company_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}