using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class TradeRepository : ITradeRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string TradeCache = "TradeData";
	private const string DistinctTradeCache = "DistinctTradeData";

	public TradeRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<TradeModel>> GetTrades(int pageNumber)
	{
		PaginatedListModel<TradeModel> output = _cache.Get<PaginatedListModel<TradeModel>>(TradeCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<TradeModel, dynamic>("USP_Trade_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<TradeModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(TradeCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(TradeCache);
			if (keys is null)
				keys = new List<string> { TradeCache + pageNumber };
			else
				keys.Add(TradeCache + pageNumber);
			_cache.Set(TradeCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<TradeModel>> GetDistinctTrades()
	{
		var output = _cache.Get<List<TradeModel>>(DistinctTradeCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<TradeModel, dynamic>("USP_Trade_GetDistinct", new { });
			_cache.Set(DistinctTradeCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<TradeModel> GetTradeById(int TradeId)
	{
		return (await _dataAccessHelper.QueryData<TradeModel, dynamic>("USP_Trade_GetById", new { Id = TradeId })).FirstOrDefault();
	}

	public async Task<TradeModel> GetTradeByName(string TradeName)
	{
		return (await _dataAccessHelper.QueryData<TradeModel, dynamic>("USP_Trade_GetByName", new { Name = TradeName })).FirstOrDefault();
	}

	public async Task<int> InsertTrade(TradeModel Trade, LogModel logModel)
	{
		ClearCache(TradeCache);
		ClearCache(DistinctTradeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("TradeName", Trade.TradeName);
		p.Add("Description", Trade.Description);
		p.Add("CreatedBy", Trade.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Trade_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateTrade(TradeModel Trade, LogModel logModel)
	{
		ClearCache(TradeCache);
		ClearCache(DistinctTradeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("TradeId", Trade.TradeId);
		p.Add("TradeName", Trade.TradeName);
		p.Add("Description", Trade.Description);
		
		p.Add("LastModifiedBy", Trade.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Trade_Update", p);
	}

	public async Task DeleteTrade(int TradeId, LogModel logModel)
	{
		ClearCache(TradeCache);
		ClearCache(DistinctTradeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", TradeId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Trade_Delete", p);
	}

	public async Task<List<TradeModel>> Export()
	{
		return await _dataAccessHelper.QueryData<TradeModel, dynamic>("USP_Trade_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case TradeCache:
				var keys = _cache.Get<List<string>>(TradeCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(TradeCache);
				}
				break;
			case DistinctTradeCache:
					_cache.Remove(DistinctTradeCache);
				break;
			default:
				break;
		}
	}
	#endregion
}