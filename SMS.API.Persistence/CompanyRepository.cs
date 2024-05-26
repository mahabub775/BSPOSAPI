using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CompanyRepository : ICompanyRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CompanyCache = "CompanyData";
	private const string DistinctCompanyCache = "DistinctCompanyData";

	public CompanyRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<CompanyModel>> GetCompanys(int pageNumber)
	{
		PaginatedListModel<CompanyModel> output = _cache.Get<PaginatedListModel<CompanyModel>>(CompanyCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<CompanyModel, dynamic>("USP_Company_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<CompanyModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(CompanyCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(CompanyCache);
			if (keys is null)
				keys = new List<string> { CompanyCache + pageNumber };
			else
				keys.Add(CompanyCache + pageNumber);
			_cache.Set(CompanyCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<CompanyModel>> GetDistinctCompanys()
	{
		var output = _cache.Get<List<CompanyModel>>(DistinctCompanyCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CompanyModel, dynamic>("USP_Company_GetDistinct", new { });
			_cache.Set(DistinctCompanyCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<CompanyModel> GetCompanyById(int CompanyId)
	{
		return (await _dataAccessHelper.QueryData<CompanyModel, dynamic>("USP_Company_GetById", new { Id = CompanyId })).FirstOrDefault();
	}

	public async Task<CompanyModel> GetCompanyByName(string CompanyName)
	{
		return (await _dataAccessHelper.QueryData<CompanyModel, dynamic>("USP_Company_GetByName", new { Name = CompanyName })).FirstOrDefault();
	}

	public async Task<int> InsertCompany(CompanyModel Company, LogModel logModel)
	{
		ClearCache(CompanyCache);
		ClearCache(DistinctCompanyCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("CompanyName", Company.CompanyName);
		p.Add("Description", Company.Description);
		p.Add("CreatedBy", Company.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Company_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCompany(CompanyModel Company, LogModel logModel)
	{
		ClearCache(CompanyCache);
		ClearCache(DistinctCompanyCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("CompanyId", Company.CompanyId);
		p.Add("CompanyName", Company.CompanyName);
		p.Add("Description", Company.Description);
		
		p.Add("LastModifiedBy", Company.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Company_Update", p);
	}

	public async Task DeleteCompany(int CompanyId, LogModel logModel)
	{
		ClearCache(CompanyCache);
		ClearCache(DistinctCompanyCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CompanyId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Company_Delete", p);
	}

	public async Task<List<CompanyModel>> Export()
	{
		return await _dataAccessHelper.QueryData<CompanyModel, dynamic>("USP_Company_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CompanyCache:
				var keys = _cache.Get<List<string>>(CompanyCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CompanyCache);
				}
				break;
			
			case DistinctCompanyCache:
						_cache.Remove(DistinctCompanyCache);
			
				break;
			default:
				break;
		}
	}
	#endregion
}