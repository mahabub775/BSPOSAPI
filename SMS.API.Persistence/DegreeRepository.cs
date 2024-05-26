using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class DegreeRepository : IDegreeRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string DegreeCache = "DegreeData";
	private const string DistinctDegreeCache = "DistinctDegreeData";

	public DegreeRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<DegreeModel>> GetDegrees(int pageNumber)
	{
		PaginatedListModel<DegreeModel> output = _cache.Get<PaginatedListModel<DegreeModel>>(DegreeCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<DegreeModel, dynamic>("USP_Degree_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<DegreeModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(DegreeCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(DegreeCache);
			if (keys is null)
				keys = new List<string> { DegreeCache + pageNumber };
			else
				keys.Add(DegreeCache + pageNumber);
			_cache.Set(DegreeCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<DegreeModel>> GetDistinctDegrees()
	{
		var output = _cache.Get<List<DegreeModel>>(DistinctDegreeCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<DegreeModel, dynamic>("USP_Degree_GetDistinct", new { });
			_cache.Set(DistinctDegreeCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<DegreeModel> GetDegreeById(int DegreeId)
	{
		return (await _dataAccessHelper.QueryData<DegreeModel, dynamic>("USP_Degree_GetById", new { Id = DegreeId })).FirstOrDefault();
	}

	public async Task<DegreeModel> GetDegreeByName(string DegreeName)
	{
		return (await _dataAccessHelper.QueryData<DegreeModel, dynamic>("USP_Degree_GetByName", new { Name = DegreeName })).FirstOrDefault();
	}

	public async Task<int> InsertDegree(DegreeModel Degree, LogModel logModel)
	{
		ClearCache(DegreeCache);
		ClearCache(DistinctDegreeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("DegreeName", Degree.DegreeName);
		p.Add("Description", Degree.Description);
		p.Add("CreatedBy", Degree.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Degree_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateDegree(DegreeModel Degree, LogModel logModel)
	{
		ClearCache(DegreeCache);
		ClearCache(DistinctDegreeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("DegreeId", Degree.DegreeId);
		p.Add("DegreeName", Degree.DegreeName);
		p.Add("Description", Degree.Description);
		
		p.Add("LastModifiedBy", Degree.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Degree_Update", p);
	}

	public async Task DeleteDegree(int DegreeId, LogModel logModel)
	{
		ClearCache(DegreeCache);
		ClearCache(DistinctDegreeCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DegreeId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Degree_Delete", p);
	}

	public async Task<List<DegreeModel>> Export()
	{
		return await _dataAccessHelper.QueryData<DegreeModel, dynamic>("USP_Degree_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case DegreeCache:
				var keys = _cache.Get<List<string>>(DegreeCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(DegreeCache);
				}
				break;
			case DistinctDegreeCache:
					_cache.Remove(DistinctDegreeCache);
				break;
			default:
				break;
		}
	}
	#endregion
}