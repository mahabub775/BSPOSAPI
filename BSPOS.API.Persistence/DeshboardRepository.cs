using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class DeshboardRepository : IDeshboardRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;


	public DeshboardRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<DeshboardModel> GetDeshboardData()
	{
		return (await _dataAccessHelper.QueryData<DeshboardModel, dynamic>("USP_Deshboard", new { })).FirstOrDefault();
	}

	#endregion

	#region "Helper Methods"
	
	#endregion
}