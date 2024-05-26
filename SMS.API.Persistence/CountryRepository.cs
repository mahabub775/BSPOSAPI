using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CountryRepository : ICountryRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CountryCache = "CountryData";
	private const string DistinctCountryCache = "DistinctCountryData";

	public CountryRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<CountryModel>> GetCountrys(int pageNumber)
	{
		PaginatedListModel<CountryModel> output = _cache.Get<PaginatedListModel<CountryModel>>(CountryCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<CountryModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(CountryCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(CountryCache);
			if (keys is null)
				keys = new List<string> { CountryCache + pageNumber };
			else
				keys.Add(CountryCache + pageNumber);
			_cache.Set(CountryCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<CountryModel>> GetDistinctCountrys()
	{
		var output = _cache.Get<List<CountryModel>>(DistinctCountryCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_GetDistinct", new { });
			_cache.Set(DistinctCountryCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<CountryModel> GetCountryById(int CountryId)
	{
		return (await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_GetById", new { Id = CountryId })).FirstOrDefault();
	}

	public async Task<CountryModel> GetCountryByName(string CountryName)
	{
		return (await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_GetByName", new { Name = CountryName })).FirstOrDefault();
	}

	public async Task<int> InsertCountry(CountryModel Country, LogModel logModel)
	{
		ClearCache(CountryCache);
		ClearCache(DistinctCountryCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("Code", Country.Code);
		p.Add("Name", Country.Name);
		p.Add("CreatedBy", Country.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Country_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCountry(CountryModel Country, LogModel logModel)
	{
		ClearCache(CountryCache);
		ClearCache(DistinctCountryCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("CountryId", Country.CountryId);
		p.Add("Code", Country.Code);
		p.Add("Name", Country.Name);
		p.Add("LastModifiedBy", Country.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Country_Update", p);
	}

	public async Task DeleteCountry(int CountryId, LogModel logModel)
	{
		ClearCache(CountryCache);
		ClearCache(DistinctCountryCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CountryId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Country_Delete", p);
	}

	public async Task<List<CountryModel>> Export()
	{
		return await _dataAccessHelper.QueryData<CountryModel, dynamic>("USP_Country_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CountryCache:
				var keys = _cache.Get<List<string>>(CountryCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CountryCache);
				}
				break;
			
			case DistinctCountryCache:
				
					_cache.Remove(DistinctCountryCache);
	
				break;
			default:
				break;
		}
	}
	#endregion
}