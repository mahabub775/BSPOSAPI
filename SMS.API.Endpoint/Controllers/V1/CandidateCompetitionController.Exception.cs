﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CandidateCompetitionController
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

			if (returningFunction.Method.Name.Contains("GetCandidateCompetitionsByCandidateId"))
				Messages = ExceptionMessages.CandidateCompetition_List;

			if (returningFunction.Method.Name.Contains("GetCandidateCompetitionById"))
				Messages = ExceptionMessages.CandidateCompetition_Id;

			if (returningFunction.Method.Name.Contains("InsertCandidateCompetition"))
				Messages = ExceptionMessages.CandidateCompetition_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCandidateCompetition"))
				Messages = ExceptionMessages.CandidateCompetition_Update;

			if (returningFunction.Method.Name.Contains("DeleteCandidateCompetition"))
				Messages = ExceptionMessages.CandidateCompetition_Delete;

	


			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}