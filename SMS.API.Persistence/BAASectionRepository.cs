using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class BAASectionRepository : IBAASectionRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string BAASectionCache = "BAASectionData";
	private const string DistinctBAASectionCache = "DistinctBAASectionData";

	public BAASectionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<BAASectionModel>> GetBAASections(int pageNumber)
	{
		PaginatedListModel<BAASectionModel> output = _cache.Get<PaginatedListModel<BAASectionModel>>(BAASectionCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<BAASectionModel, dynamic>("USP_BAASection_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<BAASectionModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(BAASectionCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(BAASectionCache);
			if (keys is null)
				keys = new List<string> { BAASectionCache + pageNumber };
			else
				keys.Add(BAASectionCache + pageNumber);
			_cache.Set(BAASectionCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<BAASectionModel>> GetDistinctBAASections()
	{
		var output = _cache.Get<List<BAASectionModel>>(DistinctBAASectionCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<BAASectionModel, dynamic>("USP_BAASection_GetDistinct", new { });
			_cache.Set(DistinctBAASectionCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<BAASectionModel> GetBAASectionById(int BAASectionId)
	{
		return (await _dataAccessHelper.QueryData<BAASectionModel, dynamic>("USP_BAASection_GetById", new { Id = BAASectionId })).FirstOrDefault();
	}

	public async Task<BAASectionModel> GetBAASectionByName(string BAASectionName)
	{
		return (await _dataAccessHelper.QueryData<BAASectionModel, dynamic>("USP_BAASection_GetByName", new { Name = BAASectionName })).FirstOrDefault();
	}

	public async Task<int> InsertBAASection(BAASectionModel BAASection, LogModel logModel)
	{
		ClearCache(BAASectionCache);
		ClearCache(DistinctBAASectionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("BAASectionName", BAASection.BAASectionName);
		p.Add("Description", BAASection.Description);
		p.Add("CreatedBy", BAASection.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_BAASection_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateBAASection(BAASectionModel BAASection, LogModel logModel)
	{
		ClearCache(BAASectionCache);
		ClearCache(DistinctBAASectionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("BAASectionId", BAASection.BAASectionId);
		p.Add("BAASectionName", BAASection.BAASectionName);
		p.Add("Description", BAASection.Description);
		
		p.Add("LastModifiedBy", BAASection.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_BAASection_Update", p);
	}

	public async Task DeleteBAASection(int BAASectionId, LogModel logModel)
	{
		ClearCache(BAASectionCache);
		ClearCache(DistinctBAASectionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", BAASectionId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_BAASection_Delete", p);
	}

	public async Task<List<BAASectionModel>> Export()
	{
		return await _dataAccessHelper.QueryData<BAASectionModel, dynamic>("USP_BAASection_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case BAASectionCache:
				var keys = _cache.Get<List<string>>(BAASectionCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(BAASectionCache);
				}
				break;
			case DistinctBAASectionCache:
					_cache.Remove(DistinctBAASectionCache);
				break;
			default:
				break;
		}
	}
	#endregion
}