using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class ApplicantRoleMappingController
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

			if (returningFunction.Method.Name.Contains("GetApplicantRoleMappings"))
				Messages = ExceptionMessages.ApplicantRoleMapping_List;



			if (returningFunction.Method.Name.Contains("GetApplicantRoleMappingById"))
				Messages = ExceptionMessages.ApplicantRoleMapping_Id;

			if (returningFunction.Method.Name.Contains("InsertApplicantRoleMapping"))
				Messages = ExceptionMessages.ApplicantRoleMapping_Insert;

			if (returningFunction.Method.Name.Contains("UpdateApplicantRoleMapping"))
				Messages = ExceptionMessages.ApplicantRoleMapping_Update;

			if (returningFunction.Method.Name.Contains("DeleteApplicantRoleMapping"))
				Messages = ExceptionMessages.ApplicantRoleMapping_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.ApplicantRoleMapping_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}