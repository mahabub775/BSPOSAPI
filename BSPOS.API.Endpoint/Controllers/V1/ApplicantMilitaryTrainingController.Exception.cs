using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantMilitaryTrainingController
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

			if (returningFunction.Method.Name.Contains("GetApplicantMilitaryTrainingsByApplicantId"))
				Messages = ExceptionMessages.ApplicantMilitaryTraining_List;

			if (returningFunction.Method.Name.Contains("GetApplicantMilitaryTrainingById"))
				Messages = ExceptionMessages.ApplicantMilitaryTraining_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantMilitaryTraining"))
				Messages = ExceptionMessages.ApplicantMilitaryTraining_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantMilitaryTraining"))
				Messages = ExceptionMessages.ApplicantMilitaryTraining_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantMilitaryTraining"))
				Messages = ExceptionMessages.ApplicantMilitaryTraining_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}