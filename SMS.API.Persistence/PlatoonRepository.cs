using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class PlatoonRepository : IPlatoonRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string PlatoonCache = "PlatoonData";
	private const string DistinctPlatoonCache = "DistinctPlatoonData";

	public PlatoonRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<PlatoonModel>> GetPlatoons(int pageNumber)
	{
		PaginatedListModel<PlatoonModel> output = _cache.Get<PaginatedListModel<PlatoonModel>>(PlatoonCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<PlatoonModel, dynamic>("USP_Platoon_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<PlatoonModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(PlatoonCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(PlatoonCache);
			if (keys is null)
				keys = new List<string> { PlatoonCache + pageNumber };
			else
				keys.Add(PlatoonCache + pageNumber);
			_cache.Set(PlatoonCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<PlatoonModel>> GetDistinctPlatoons()
	{
		var output = _cache.Get<List<PlatoonModel>>(DistinctPlatoonCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<PlatoonModel, dynamic>("USP_Platoon_GetDistinct", new { });
			_cache.Set(DistinctPlatoonCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<PlatoonModel> GetPlatoonById(int PlatoonId)
	{
		return (await _dataAccessHelper.QueryData<PlatoonModel, dynamic>("USP_Platoon_GetById", new { Id = PlatoonId })).FirstOrDefault();
	}

	public async Task<PlatoonModel> GetPlatoonByName(string PlatoonName)
	{
		return (await _dataAccessHelper.QueryData<PlatoonModel, dynamic>("USP_Platoon_GetByName", new { Name = PlatoonName })).FirstOrDefault();
	}

	public async Task<int> InsertPlatoon(PlatoonModel Platoon, LogModel logModel)
	{
		ClearCache(PlatoonCache);
		ClearCache(DistinctPlatoonCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("PlatoonName", Platoon.PlatoonName);
		p.Add("Description", Platoon.Description);
		p.Add("CreatedBy", Platoon.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Platoon_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdatePlatoon(PlatoonModel Platoon, LogModel logModel)
	{
		ClearCache(PlatoonCache);
		ClearCache(DistinctPlatoonCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("PlatoonId", Platoon.PlatoonId);
		p.Add("PlatoonName", Platoon.PlatoonName);
		p.Add("Description", Platoon.Description);
		
		p.Add("LastModifiedBy", Platoon.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Platoon_Update", p);
	}

	public async Task DeletePlatoon(int PlatoonId, LogModel logModel)
	{
		ClearCache(PlatoonCache);
		ClearCache(DistinctPlatoonCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", PlatoonId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Platoon_Delete", p);
	}

	public async Task<List<PlatoonModel>> Export()
	{
		return await _dataAccessHelper.QueryData<PlatoonModel, dynamic>("USP_Platoon_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case PlatoonCache:
				var keys = _cache.Get<List<string>>(PlatoonCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(PlatoonCache);
				}
				break;
			case DistinctPlatoonCache:
					_cache.Remove(DistinctPlatoonCache);
				break;
			default:
				break;
		}
	}
	#endregion
}