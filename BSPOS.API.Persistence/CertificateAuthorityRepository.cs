using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CertificateAuthorityRepository : ICertificateAuthorityRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CertificateAuthorityCache = "CertificateAuthorityData";
	private const string DistinctCertificateAuthorityCache = "DistinctCertificateAuthorityData";

	public CertificateAuthorityRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<CertificateAuthorityModel>> GetCertificateAuthoritys(int pageNumber)
	{
		PaginatedListModel<CertificateAuthorityModel> output = _cache.Get<PaginatedListModel<CertificateAuthorityModel>>(CertificateAuthorityCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<CertificateAuthorityModel, dynamic>("USP_CertificateAuthority_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<CertificateAuthorityModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(CertificateAuthorityCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(CertificateAuthorityCache);
			if (keys is null)
				keys = new List<string> { CertificateAuthorityCache + pageNumber };
			else
				keys.Add(CertificateAuthorityCache + pageNumber);
			_cache.Set(CertificateAuthorityCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<CertificateAuthorityModel>> GetDistinctCertificateAuthoritys()
	{
		var output = _cache.Get<List<CertificateAuthorityModel>>(DistinctCertificateAuthorityCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CertificateAuthorityModel, dynamic>("USP_CertificateAuthority_GetDistinct", new { });
			_cache.Set(DistinctCertificateAuthorityCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<CertificateAuthorityModel> GetCertificateAuthorityById(int CertificateAuthorityId)
	{
		return (await _dataAccessHelper.QueryData<CertificateAuthorityModel, dynamic>("USP_CertificateAuthority_GetById", new { Id = CertificateAuthorityId })).FirstOrDefault();
	}

	public async Task<CertificateAuthorityModel> GetCertificateAuthorityByName(string CertificateAuthorityName)
	{
		return (await _dataAccessHelper.QueryData<CertificateAuthorityModel, dynamic>("USP_CertificateAuthority_GetByName", new { Name = CertificateAuthorityName })).FirstOrDefault();
	}

	public async Task<int> InsertCertificateAuthority(CertificateAuthorityModel CertificateAuthority, LogModel logModel)
	{
		ClearCache(CertificateAuthorityCache);
		ClearCache(DistinctCertificateAuthorityCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("CertificateAuthorityName", CertificateAuthority.CertificateAuthorityName);
		p.Add("Description", CertificateAuthority.Description);
		p.Add("CreatedBy", CertificateAuthority.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CertificateAuthority_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCertificateAuthority(CertificateAuthorityModel CertificateAuthority, LogModel logModel)
	{
		ClearCache(CertificateAuthorityCache);
		ClearCache(DistinctCertificateAuthorityCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("CertificateAuthorityId", CertificateAuthority.CertificateAuthorityId);
		p.Add("CertificateAuthorityName", CertificateAuthority.CertificateAuthorityName);
		p.Add("Description", CertificateAuthority.Description);
		
		p.Add("LastModifiedBy", CertificateAuthority.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CertificateAuthority_Update", p);
	}

	public async Task DeleteCertificateAuthority(int CertificateAuthorityId, LogModel logModel)
	{
		ClearCache(CertificateAuthorityCache);
		ClearCache(DistinctCertificateAuthorityCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CertificateAuthorityId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CertificateAuthority_Delete", p);
	}

	public async Task<List<CertificateAuthorityModel>> Export()
	{
		return await _dataAccessHelper.QueryData<CertificateAuthorityModel, dynamic>("USP_CertificateAuthority_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CertificateAuthorityCache:
				var keys = _cache.Get<List<string>>(CertificateAuthorityCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CertificateAuthorityCache);
				}
				break;		
			case DistinctCertificateAuthorityCache:
					_cache.Remove(DistinctCertificateAuthorityCache);
				
				break;
			default:
				break;
		}
	}
	#endregion
}