using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CivilInstitutionController
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

			if (returningFunction.Method.Name.Contains("GetCivilInstitutions"))
				Messages = ExceptionMessages.CivilInstitution_List;

			if (returningFunction.Method.Name.Contains("GetDistinctCivilInstitutions"))
				Messages = ExceptionMessages.CivilInstitution_List;

			if (returningFunction.Method.Name.Contains("GetCivilInstitutionById"))
				Messages = ExceptionMessages.CivilInstitution_Id;

			if (returningFunction.Method.Name.Contains("InsertCivilInstitution"))
				Messages = ExceptionMessages.CivilInstitution_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCivilInstitution"))
				Messages = ExceptionMessages.CivilInstitution_Update;

			if (returningFunction.Method.Name.Contains("DeleteCivilInstitution"))
				Messages = ExceptionMessages.CivilInstitution_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.CivilInstitution_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}