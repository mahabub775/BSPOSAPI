﻿using Microsoft.AspNetCore.Http;

namespace BSPOS.Infrastructure;

public class Utility
{
	public static string GetIPAddress(HttpRequest request)
	{
		if (request != null)
		{
			return request.HttpContext.Connection.RemoteIpAddress.ToString();
		}

		return "";
	}
}