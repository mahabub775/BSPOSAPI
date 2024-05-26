using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantDisciplineController
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

			if (returningFunction.Method.Name.Contains("GetApplicantDisciplinesByApplicantId"))
				Messages = ExceptionMessages.ApplicantDiscipline_List;

			if (returningFunction.Method.Name.Contains("GetApplicantDisciplineById"))
				Messages = ExceptionMessages.ApplicantDiscipline_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantDiscipline"))
				Messages = ExceptionMessages.ApplicantDiscipline_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantDiscipline"))
				Messages = ExceptionMessages.ApplicantDiscipline_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantDiscipline"))
				Messages = ExceptionMessages.ApplicantDiscipline_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}