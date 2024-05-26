using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class MilitaryInstitutionController
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

			if (returningFunction.Method.Name.Contains("GetMilitaryInstitutions"))
				Messages = ExceptionMessages.MilitaryInstitution_List;

			if (returningFunction.Method.Name.Contains("GetDistinctMilitaryInstitutions"))
				Messages = ExceptionMessages.MilitaryInstitution_List;

			if (returningFunction.Method.Name.Contains("GetMilitaryInstitutionById"))
				Messages = ExceptionMessages.MilitaryInstitution_Id;

			if (returningFunction.Method.Name.Contains("InsertMilitaryInstitution"))
				Messages = ExceptionMessages.MilitaryInstitution_Insert;

			if (returningFunction.Method.Name.Contains("UpdateMilitaryInstitution"))
				Messages = ExceptionMessages.MilitaryInstitution_Update;

			if (returningFunction.Method.Name.Contains("DeleteMilitaryInstitution"))
				Messages = ExceptionMessages.MilitaryInstitution_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.MilitaryInstitution_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}