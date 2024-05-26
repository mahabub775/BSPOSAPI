using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateAddlQualificationController
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

			if (returningFunction.Method.Name.Contains("GetCandidateAddlQualificationsByCandidateId"))
				Messages = ExceptionMessages.CandidateAddlQualification_List;

			if (returningFunction.Method.Name.Contains("GetCandidateAddlQualificationById"))
				Messages = ExceptionMessages.CandidateAddlQualification_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateAddlQualification"))
				Messages = ExceptionMessages.CandidateAddlQualification_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateAddlQualification"))
				Messages = ExceptionMessages.CandidateAddlQualification_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateAddlQualification"))
				Messages = ExceptionMessages.CandidateAddlQualification_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}