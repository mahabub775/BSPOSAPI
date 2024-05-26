using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CivilInstitutionRepository : ICivilInstitutionRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CivilInstitutionCache = "CivilInstitutionData";
	private const string DistinctCivilInstitutionCache = "DistinctCivilInstitutionData";

	public CivilInstitutionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<CivilInstitutionModel>> GetCivilInstitutions(int pageNumber)
	{
		PaginatedListModel<CivilInstitutionModel> output = _cache.Get<PaginatedListModel<CivilInstitutionModel>>(CivilInstitutionCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<CivilInstitutionModel, dynamic>("USP_CivilInstitution_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<CivilInstitutionModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(CivilInstitutionCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(CivilInstitutionCache);
			if (keys is null)
				keys = new List<string> { CivilInstitutionCache + pageNumber };
			else
				keys.Add(CivilInstitutionCache + pageNumber);
			_cache.Set(CivilInstitutionCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<CivilInstitutionModel>> GetDistinctCivilInstitutions()
	{
		var output = _cache.Get<List<CivilInstitutionModel>>(DistinctCivilInstitutionCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CivilInstitutionModel, dynamic>("USP_CivilInstitution_GetDistinct", new { });
			_cache.Set(DistinctCivilInstitutionCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}
	public async Task<List<CivilInstitutionModel>> GetTopCivilEducations()
	{
		var output = _cache.Get<List<CivilInstitutionModel>>(DistinctCivilInstitutionCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CivilInstitutionModel, dynamic>("USP_TopCivilEducation", new { });
			_cache.Set(DistinctCivilInstitutionCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<CivilInstitutionModel> GetCivilInstitutionById(int CivilInstitutionId)
	{
		return (await _dataAccessHelper.QueryData<CivilInstitutionModel, dynamic>("USP_CivilInstitution_GetById", new { Id = CivilInstitutionId })).FirstOrDefault();
	}

	public async Task<CivilInstitutionModel> GetCivilInstitutionByName(string CivilInstitutionName)
	{
		return (await _dataAccessHelper.QueryData<CivilInstitutionModel, dynamic>("USP_CivilInstitution_GetByName", new { Name = CivilInstitutionName })).FirstOrDefault();
	}

	public async Task<int> InsertCivilInstitution(CivilInstitutionModel CivilInstitution, LogModel logModel)
	{
		ClearCache(CivilInstitutionCache);
		ClearCache(DistinctCivilInstitutionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("CivilInstitutionId", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("InstitutionName", CivilInstitution.InstitutionName);
		p.Add("Description", CivilInstitution.Description);
		p.Add("CreatedBy", CivilInstitution.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CivilInstitution_Insert", p);
		return p.Get<int>("CivilInstitutionId");
	}

	public async Task UpdateCivilInstitution(CivilInstitutionModel CivilInstitution, LogModel logModel)
	{
		ClearCache(CivilInstitutionCache);
		ClearCache(DistinctCivilInstitutionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("CivilInstitutionId", CivilInstitution.CivilInstitutionId);
		p.Add("InstitutionName", CivilInstitution.InstitutionName);
		p.Add("Description", CivilInstitution.Description);
		
		p.Add("LastModifiedBy", CivilInstitution.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CivilInstitution_Update", p);
	}
	public async Task DeleteCivilInstitution(int CivilInstitutionId, LogModel logModel)
	{
		ClearCache(CivilInstitutionCache);
		ClearCache(DistinctCivilInstitutionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CivilInstitutionId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CivilInstitution_Delete", p);
	}

	public async Task<List<CivilInstitutionModel>> Export()
	{
		return await _dataAccessHelper.QueryData<CivilInstitutionModel, dynamic>("USP_CivilInstitution_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CivilInstitutionCache:
				var keys = _cache.Get<List<string>>(CivilInstitutionCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CivilInstitutionCache);
				}
				break;
			
			case DistinctCivilInstitutionCache:
					_cache.Remove(DistinctCivilInstitutionCache);
				break;
			default:
				break;
		}
	}
	#endregion
}