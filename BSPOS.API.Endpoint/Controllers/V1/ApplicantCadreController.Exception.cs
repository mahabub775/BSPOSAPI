using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantCadreController
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

			if (returningFunction.Method.Name.Contains("GetApplicantCadresByApplicantId"))
				Messages = ExceptionMessages.ApplicantCadre_List;

			if (returningFunction.Method.Name.Contains("GetApplicantCadreById"))
				Messages = ExceptionMessages.ApplicantCadre_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantCadre"))
				Messages = ExceptionMessages.ApplicantCadre_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantCadre"))
				Messages = ExceptionMessages.ApplicantCadre_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantCadre"))
				Messages = ExceptionMessages.ApplicantCadre_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}