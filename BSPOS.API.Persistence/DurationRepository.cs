using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class DurationRepository : IDurationRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string DurationCache = "DurationData";
	private const string DistinctDurationCache = "DistinctDurationData";

	public DurationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<DurationModel>> GetDurations(int pageNumber)
	{
		PaginatedListModel<DurationModel> output = _cache.Get<PaginatedListModel<DurationModel>>(DurationCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<DurationModel, dynamic>("USP_Duration_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<DurationModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(DurationCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(DurationCache);
			if (keys is null)
				keys = new List<string> { DurationCache + pageNumber };
			else
				keys.Add(DurationCache + pageNumber);
			_cache.Set(DurationCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<DurationModel>> GetDistinctDurations()
	{
		var output = _cache.Get<List<DurationModel>>(DistinctDurationCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<DurationModel, dynamic>("USP_Duration_GetDistinct", new { });
			_cache.Set(DistinctDurationCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<DurationModel> GetDurationById(int DurationId)
	{
		return (await _dataAccessHelper.QueryData<DurationModel, dynamic>("USP_Duration_GetById", new { Id = DurationId })).FirstOrDefault();
	}

	public async Task<DurationModel> GetDurationByName(string DurationName)
	{
		return (await _dataAccessHelper.QueryData<DurationModel, dynamic>("USP_Duration_GetByName", new { Name = DurationName })).FirstOrDefault();
	}

	public async Task<int> InsertDuration(DurationModel Duration, LogModel logModel)
	{
		ClearCache(DurationCache);
		ClearCache(DistinctDurationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("DurationName", Duration.DurationName);
		p.Add("Description", Duration.Description);
		p.Add("CreatedBy", Duration.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Duration_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateDuration(DurationModel Duration, LogModel logModel)
	{
		ClearCache(DurationCache);
		ClearCache(DistinctDurationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("DurationId", Duration.DurationId);
		p.Add("DurationName", Duration.DurationName);
		p.Add("Description", Duration.Description);
		
		p.Add("LastModifiedBy", Duration.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Duration_Update", p);
	}

	public async Task DeleteDuration(int DurationId, LogModel logModel)
	{
		ClearCache(DurationCache);
		ClearCache(DistinctDurationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DurationId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Duration_Delete", p);
	}

	public async Task<List<DurationModel>> Export()
	{
		return await _dataAccessHelper.QueryData<DurationModel, dynamic>("USP_Duration_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case DurationCache:
				var keys = _cache.Get<List<string>>(DurationCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(DurationCache);
				}
				break;
			
			case DistinctDurationCache:
					_cache.Remove(DistinctDurationCache);
		
				break;
			default:
				break;
		}
	}
	#endregion
}