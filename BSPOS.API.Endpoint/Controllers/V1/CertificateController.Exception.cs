using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace SMS.API.Endpoint.Controllers.V1;

public partial class CertificateController
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

			if (returningFunction.Method.Name.Contains("GetCertificates"))
				Messages = ExceptionMessages.Certificate_List;

			if (returningFunction.Method.Name.Contains("GetDistinctCertificates"))
				Messages = ExceptionMessages.Certificate_List;

			if (returningFunction.Method.Name.Contains("GetCertificateById"))
				Messages = ExceptionMessages.Certificate_Id;

			if (returningFunction.Method.Name.Contains("InsertCertificate"))
				Messages = ExceptionMessages.Certificate_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCertificate"))
				Messages = ExceptionMessages.Certificate_Update;

			if (returningFunction.Method.Name.Contains("DeleteCertificate"))
				Messages = ExceptionMessages.Certificate_Delete;

	
			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Certificate_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}