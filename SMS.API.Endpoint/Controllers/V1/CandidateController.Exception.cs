using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateController
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

			if (returningFunction.Method.Name.Contains("GetCandidates"))
				Messages = ExceptionMessages.Candidate_List;

			if (returningFunction.Method.Name.Contains("GetDistinctCandidates"))
				Messages = ExceptionMessages.Candidate_List;

			if (returningFunction.Method.Name.Contains("GetCandidateById"))
				Messages = ExceptionMessages.Candidate_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidate"))
				Messages = ExceptionMessages.Candidate_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidate"))
				Messages = ExceptionMessages.Candidate_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidate"))
				Messages = ExceptionMessages.Candidate_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Candidate_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}