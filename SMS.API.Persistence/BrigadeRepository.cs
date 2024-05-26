using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class BrigadeRepository : IBrigadeRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string BrigadeCache = "BrigadeData";
	private const string DistinctBrigadeCache = "DistinctBrigadeData";

	public BrigadeRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<BrigadeModel>> GetBrigades(int pageNumber)
	{
		PaginatedListModel<BrigadeModel> output = _cache.Get<PaginatedListModel<BrigadeModel>>(BrigadeCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<BrigadeModel, dynamic>("USP_Brigade_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<BrigadeModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(BrigadeCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(BrigadeCache);
			if (keys is null)
				keys = new List<string> { BrigadeCache + pageNumber };
			else
				keys.Add(BrigadeCache + pageNumber);
			_cache.Set(BrigadeCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<BrigadeModel>> GetDistinctBrigades()
	{
		var output = _cache.Get<List<BrigadeModel>>(DistinctBrigadeCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<BrigadeModel, dynamic>("USP_Brigade_GetDistinct", new { });
			_cache.Set(DistinctBrigadeCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<BrigadeModel> GetBrigadeById(int BrigadeId)
	{
		return (await _dataAccessHelper.QueryData<BrigadeModel, dynamic>("USP_Brigade_GetById", new { Id = BrigadeId })).FirstOrDefault();
	}

	public async Task<BrigadeModel> GetBrigadeByName(string BrigadeName)
	{
		return (await _dataAccessHelper.QueryData<BrigadeModel, dynamic>("USP_Brigade_GetByName", new { Name = BrigadeName })).FirstOrDefault();
	}

	public async Task<int> InsertBrigade(BrigadeModel Brigade, LogModel logModel)
	{
		ClearCache(BrigadeCache);
		ClearCache(DistinctBrigadeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("BrigadeName", Brigade.BrigadeName);
		p.Add("Description", Brigade.Description);
		p.Add("CreatedBy", Brigade.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Brigade_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateBrigade(BrigadeModel Brigade, LogModel logModel)
	{
		ClearCache(BrigadeCache);
		ClearCache(DistinctBrigadeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("BrigadeId", Brigade.BrigadeId);
		p.Add("BrigadeName", Brigade.BrigadeName);
		p.Add("Description", Brigade.Description);
		
		p.Add("LastModifiedBy", Brigade.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Brigade_Update", p);
	}

	public async Task DeleteBrigade(int BrigadeId, LogModel logModel)
	{
		ClearCache(BrigadeCache);
		ClearCache(DistinctBrigadeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", BrigadeId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Brigade_Delete", p);
	}

	public async Task<List<BrigadeModel>> Export()
	{
		return await _dataAccessHelper.QueryData<BrigadeModel, dynamic>("USP_Brigade_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case BrigadeCache:
				var keys = _cache.Get<List<string>>(BrigadeCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(BrigadeCache);
				}
				break;
			
			case DistinctBrigadeCache:
					_cache.Remove(DistinctBrigadeCache);
				
				break;
			default:
				break;
		}
	}
	#endregion
}