using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BSPOS.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace BSPOS.API.Endpoint.Controllers.V1;

public partial class UserController
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

			//if (returningFunction.Method.Name.Contains("GetCategories"))
			//	Messages = ExceptionMessages.User_List;

			//if (returningFunction.Method.Name.Contains("GetDistinctCategories"))
			//	Messages = ExceptionMessages.User_List;

			//if (returningFunction.Method.Name.Contains("GetUserById"))
			//	Messages = ExceptionMessages.User_Id;

			//if (returningFunction.Method.Name.Contains("InsertUser"))
			//	Messages = ExceptionMessages.User_Insert;

			//if (returningFunction.Method.Name.Contains("UpdateUser"))
			//	Messages = ExceptionMessages.User_Update;

			//if (returningFunction.Method.Name.Contains("DeleteUser"))
			//	Messages = ExceptionMessages.User_Delete;

			//if (returningFunction.Method.Name.Contains("GetCategoriesWithPies"))
			//	Messages = ExceptionMessages.User_CategoriesWithPies;

			//if (returningFunction.Method.Name.Contains("Export"))
			//	Messages = ExceptionMessages.User_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}