using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class BIAnnualRepository : IBIAnnualRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string BIAnnualCache = "BIAnnualData";
	private const string DistinctBIAnnualCache = "DistinctBIAnnualData";

	public BIAnnualRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<BIAnnualModel>> GetBIAnnuals(int pageNumber)
	{
		PaginatedListModel<BIAnnualModel> output = _cache.Get<PaginatedListModel<BIAnnualModel>>(BIAnnualCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<BIAnnualModel, dynamic>("USP_BIAnnual_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<BIAnnualModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(BIAnnualCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(BIAnnualCache);
			if (keys is null)
				keys = new List<string> { BIAnnualCache + pageNumber };
			else
				keys.Add(BIAnnualCache + pageNumber);
			_cache.Set(BIAnnualCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<BIAnnualModel>> GetDistinctBIAnnuals()
	{
		var output = _cache.Get<List<BIAnnualModel>>(DistinctBIAnnualCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<BIAnnualModel, dynamic>("USP_BIAnnual_GetDistinct", new { });
			_cache.Set(DistinctBIAnnualCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<BIAnnualModel> GetBIAnnualById(int BIAnnualId)
	{
		return (await _dataAccessHelper.QueryData<BIAnnualModel, dynamic>("USP_BIAnnual_GetById", new { Id = BIAnnualId })).FirstOrDefault();
	}

	public async Task<BIAnnualModel> GetBIAnnualByName(string BIAnnualName)
	{
		return (await _dataAccessHelper.QueryData<BIAnnualModel, dynamic>("USP_BIAnnual_GetByName", new { Name = BIAnnualName })).FirstOrDefault();
	}

	public async Task<int> InsertBIAnnual(BIAnnualModel BIAnnual, LogModel logModel)
	{
		ClearCache(BIAnnualCache);
		ClearCache(DistinctBIAnnualCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("BIAnnualName", BIAnnual.BIAnnualName);
		p.Add("Description", BIAnnual.Description);
		p.Add("CreatedBy", BIAnnual.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_BIAnnual_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateBIAnnual(BIAnnualModel BIAnnual, LogModel logModel)
	{
		ClearCache(BIAnnualCache);
		ClearCache(DistinctBIAnnualCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("BIAnnualId", BIAnnual.BIAnnualId);
		p.Add("BIAnnualName", BIAnnual.BIAnnualName);
		p.Add("Description", BIAnnual.Description);
		
		p.Add("LastModifiedBy", BIAnnual.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_BIAnnual_Update", p);
	}

	public async Task DeleteBIAnnual(int BIAnnualId, LogModel logModel)
	{
		ClearCache(BIAnnualCache);
		ClearCache(DistinctBIAnnualCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", BIAnnualId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_BIAnnual_Delete", p);
	}

	public async Task<List<BIAnnualModel>> Export()
	{
		return await _dataAccessHelper.QueryData<BIAnnualModel, dynamic>("USP_BIAnnual_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case BIAnnualCache:
				var keys = _cache.Get<List<string>>(BIAnnualCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(BIAnnualCache);
				}
				break;		
			case DistinctBIAnnualCache:
					_cache.Remove(DistinctBIAnnualCache);
				break;
			default:
				break;
		}
	}
	#endregion
}