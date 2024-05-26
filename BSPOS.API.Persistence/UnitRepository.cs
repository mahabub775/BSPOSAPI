using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class UnitRepository : IUnitRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string UnitCache = "UnitData";
	private const string DistinctUnitCache = "DistinctUnitData";

	public UnitRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<UnitModel>> GetUnits(int pageNumber)
	{
		PaginatedListModel<UnitModel> output = _cache.Get<PaginatedListModel<UnitModel>>(UnitCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<UnitModel, dynamic>("USP_Unit_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<UnitModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(UnitCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(UnitCache);
			if (keys is null)
				keys = new List<string> { UnitCache + pageNumber };
			else
				keys.Add(UnitCache + pageNumber);
			_cache.Set(UnitCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<UnitModel>> GetDistinctUnits()
	{
		var output = _cache.Get<List<UnitModel>>(DistinctUnitCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<UnitModel, dynamic>("USP_Unit_GetDistinct", new { });
			_cache.Set(DistinctUnitCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<UnitModel>> GetUnitsByBrigade(int BrigadeId)
	{
		return (await  _dataAccessHelper.QueryData<UnitModel, dynamic>("usp_GetUnitsByBrigadeId", new { BrigadeId }));
		
	}

	public async Task<UnitModel> GetUnitById(int UnitId)
	{
		return (await _dataAccessHelper.QueryData<UnitModel, dynamic>("USP_Unit_GetById", new { Id = UnitId })).FirstOrDefault();
	}

	public async Task<UnitModel> GetUnitByName(string UnitName)
	{
		return (await _dataAccessHelper.QueryData<UnitModel, dynamic>("USP_Unit_GetByName", new { Name = UnitName })).FirstOrDefault();
	}

	public async Task<int> InsertUnit(UnitModel Unit, LogModel logModel)
	{
		ClearCache(UnitCache);
		ClearCache(DistinctUnitCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("UnitName", Unit.UnitName);
		p.Add("Description", Unit.Description);
		p.Add("CreatedBy", Unit.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Unit_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateUnit(UnitModel Unit, LogModel logModel)
	{
		ClearCache(UnitCache);
		ClearCache(DistinctUnitCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("UnitId", Unit.UnitId);
		p.Add("UnitName", Unit.UnitName);
		p.Add("Description", Unit.Description);
		
		p.Add("LastModifiedBy", Unit.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Unit_Update", p);
	}

	public async Task DeleteUnit(int UnitId, LogModel logModel)
	{
		ClearCache(UnitCache);
		ClearCache(DistinctUnitCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", UnitId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Unit_Delete", p);
	}

	public async Task<List<UnitModel>> Export()
	{
		return await _dataAccessHelper.QueryData<UnitModel, dynamic>("USP_Unit_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case UnitCache:
				var keys = _cache.Get<List<string>>(UnitCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(UnitCache);
				}
				break;
			case DistinctUnitCache:
					_cache.Remove(DistinctUnitCache);
				break;
			default:
				break;
		}
	}
	#endregion
}