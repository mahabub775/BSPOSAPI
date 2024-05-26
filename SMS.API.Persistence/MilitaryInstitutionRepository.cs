using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class MilitaryInstitutionRepository : IMilitaryInstitutionRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string MilitaryInstitutionCache = "MilitaryInstitutionData";
	private const string DistinctMilitaryInstitutionCache = "DistinctMilitaryInstitutionData";

	public MilitaryInstitutionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<MilitaryInstitutionModel>> GetMilitaryInstitutions(int pageNumber)
	{
		PaginatedListModel<MilitaryInstitutionModel> output = _cache.Get<PaginatedListModel<MilitaryInstitutionModel>>(MilitaryInstitutionCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<MilitaryInstitutionModel, dynamic>("USP_MilitaryInstitution_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<MilitaryInstitutionModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(MilitaryInstitutionCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(MilitaryInstitutionCache);
			if (keys is null)
				keys = new List<string> { MilitaryInstitutionCache + pageNumber };
			else
				keys.Add(MilitaryInstitutionCache + pageNumber);
			_cache.Set(MilitaryInstitutionCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<MilitaryInstitutionModel>> GetDistinctMilitaryInstitutions()
	{
		var output = _cache.Get<List<MilitaryInstitutionModel>>(DistinctMilitaryInstitutionCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<MilitaryInstitutionModel, dynamic>("USP_MilitaryInstitution_GetDistinct", new { });
			_cache.Set(DistinctMilitaryInstitutionCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<MilitaryInstitutionModel> GetMilitaryInstitutionById(int MilitaryInstitutionId)
	{
		return (await _dataAccessHelper.QueryData<MilitaryInstitutionModel, dynamic>("USP_MilitaryInstitution_GetById", new { Id = MilitaryInstitutionId })).FirstOrDefault();
	}

	public async Task<MilitaryInstitutionModel> GetMilitaryInstitutionByName(string MilitaryInstitutionName)
	{
		return (await _dataAccessHelper.QueryData<MilitaryInstitutionModel, dynamic>("USP_MilitaryInstitution_GetByName", new { Name = MilitaryInstitutionName })).FirstOrDefault();
	}

	public async Task<int> InsertMilitaryInstitution(MilitaryInstitutionModel MilitaryInstitution, LogModel logModel)
	{
		ClearCache(MilitaryInstitutionCache);
		ClearCache(DistinctMilitaryInstitutionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("MilitaryInstitutionId", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("InstitutionName", MilitaryInstitution.InstitutionName);
		p.Add("Description", MilitaryInstitution.Description);
		p.Add("CreatedBy", MilitaryInstitution.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_MilitaryInstitution_Insert", p);
		return p.Get<int>("MilitaryInstitutionId");
	}

	public async Task UpdateMilitaryInstitution(MilitaryInstitutionModel MilitaryInstitution, LogModel logModel)
	{
		ClearCache(MilitaryInstitutionCache);
		ClearCache(DistinctMilitaryInstitutionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("MilitaryInstitutionId", MilitaryInstitution.MilitaryInstitutionId);
		p.Add("InstitutionName", MilitaryInstitution.InstitutionName);
		p.Add("Description", MilitaryInstitution.Description);
		
		p.Add("LastModifiedBy", MilitaryInstitution.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_MilitaryInstitution_Update", p);
	}

	public async Task DeleteMilitaryInstitution(int MilitaryInstitutionId, LogModel logModel)
	{
		ClearCache(MilitaryInstitutionCache);
		ClearCache(DistinctMilitaryInstitutionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", MilitaryInstitutionId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_MilitaryInstitution_Delete", p);
	}

	public async Task<List<MilitaryInstitutionModel>> Export()
	{
		return await _dataAccessHelper.QueryData<MilitaryInstitutionModel, dynamic>("USP_MilitaryInstitution_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case MilitaryInstitutionCache:
				var keys = _cache.Get<List<string>>(MilitaryInstitutionCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(MilitaryInstitutionCache);
				}
				break;
			case DistinctMilitaryInstitutionCache:
					_cache.Remove(DistinctMilitaryInstitutionCache);
				break;
			default:
				break;
		}
	}
	#endregion
}