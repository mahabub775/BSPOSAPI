using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CertificateAuthorityController
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

			if (returningFunction.Method.Name.Contains("GetCertificateAuthoritys"))
				Messages = ExceptionMessages.CertificateAuthority_List;

			if (returningFunction.Method.Name.Contains("GetDistinctCertificateAuthoritys"))
				Messages = ExceptionMessages.CertificateAuthority_List;

			if (returningFunction.Method.Name.Contains("GetCertificateAuthorityById"))
				Messages = ExceptionMessages.CertificateAuthority_Id;

			if (returningFunction.Method.Name.Contains("InsertCertificateAuthority"))
				Messages = ExceptionMessages.CertificateAuthority_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCertificateAuthority"))
				Messages = ExceptionMessages.CertificateAuthority_Update;

			if (returningFunction.Method.Name.Contains("DeleteCertificateAuthority"))
				Messages = ExceptionMessages.CertificateAuthority_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.CertificateAuthority_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}