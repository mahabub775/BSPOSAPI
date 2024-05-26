using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class RankRepository : IRankRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string RankCache = "RankData";
	private const string DistinctRankCache = "DistinctRankData";

	public RankRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<RankModel>> GetRanks(int pageNumber)
	{
		PaginatedListModel<RankModel> output = _cache.Get<PaginatedListModel<RankModel>>(RankCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<RankModel, dynamic>("USP_Rank_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<RankModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(RankCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(RankCache);
			if (keys is null)
				keys = new List<string> { RankCache + pageNumber };
			else
				keys.Add(RankCache + pageNumber);
			_cache.Set(RankCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<RankModel>> GetDistinctRanks()
	{
		var output = _cache.Get<List<RankModel>>(DistinctRankCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<RankModel, dynamic>("USP_Rank_GetDistinct", new { });
			_cache.Set(DistinctRankCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<RankModel> GetRankById(int RankId)
	{
		return (await _dataAccessHelper.QueryData<RankModel, dynamic>("USP_Rank_GetById", new { Id = RankId })).FirstOrDefault();
	}

	public async Task<RankModel> GetRankByName(string RankName)
	{
		return (await _dataAccessHelper.QueryData<RankModel, dynamic>("USP_Rank_GetByName", new { Name = RankName })).FirstOrDefault();
	}

	public async Task<int> InsertRank(RankModel Rank, LogModel logModel)
	{
		ClearCache(RankCache);
		ClearCache(DistinctRankCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("RankName", Rank.RankName);
		p.Add("Description", Rank.Description);
		p.Add("CreatedBy", Rank.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Rank_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateRank(RankModel Rank, LogModel logModel)
	{
		ClearCache(RankCache);
		ClearCache(DistinctRankCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("RankId", Rank.RankId);
		p.Add("RankName", Rank.RankName);
		p.Add("Description", Rank.Description);
		
		p.Add("LastModifiedBy", Rank.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Rank_Update", p);
	}

	public async Task DeleteRank(int RankId, LogModel logModel)
	{
		ClearCache(RankCache);
		ClearCache(DistinctRankCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", RankId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Rank_Delete", p);
	}

	public async Task<List<RankModel>> Export()
	{
		return await _dataAccessHelper.QueryData<RankModel, dynamic>("USP_Rank_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case RankCache:
				var keys = _cache.Get<List<string>>(RankCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(RankCache);
				}
				break;
			case DistinctRankCache:
					_cache.Remove(DistinctRankCache);
				break;
			default:
				break;
		}
	}
	#endregion
}