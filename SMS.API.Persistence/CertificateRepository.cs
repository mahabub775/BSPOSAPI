using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CertificateRepository : ICertificateRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CertificateCache = "CertificateData";
	private const string DistinctCertificateCache = "DistinctCertificateData";

	public CertificateRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<CertificateModel>> GetCertificates(int pageNumber)
	{
		PaginatedListModel<CertificateModel> output = _cache.Get<PaginatedListModel<CertificateModel>>(CertificateCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<CertificateModel, dynamic>("USP_Certificate_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<CertificateModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(CertificateCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(CertificateCache);
			if (keys is null)
				keys = new List<string> { CertificateCache + pageNumber };
			else
				keys.Add(CertificateCache + pageNumber);
			_cache.Set(CertificateCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<CertificateModel>> GetDistinctCertificates()
	{
		var output = _cache.Get<List<CertificateModel>>(DistinctCertificateCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CertificateModel, dynamic>("USP_Certificate_GetDistinct", new { });
			_cache.Set(DistinctCertificateCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<CertificateModel> GetCertificateById(int CertificateId)
	{
		return (await _dataAccessHelper.QueryData<CertificateModel, dynamic>("USP_Certificate_GetById", new { Id = CertificateId })).FirstOrDefault();
	}

	public async Task<CertificateModel> GetCertificateByName(string CertificateName)
	{
		return (await _dataAccessHelper.QueryData<CertificateModel, dynamic>("USP_Certificate_GetByName", new { Name = CertificateName })).FirstOrDefault();
	}

	public async Task<int> InsertCertificate(CertificateModel Certificate, LogModel logModel)
	{
		ClearCache(CertificateCache);
		ClearCache(DistinctCertificateCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("CertificateName", Certificate.CertificateName);
		p.Add("Description", Certificate.Description);
		p.Add("CreatedBy", Certificate.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Certificate_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCertificate(CertificateModel Certificate, LogModel logModel)
	{
		ClearCache(CertificateCache);
		ClearCache(DistinctCertificateCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("CertificateId", Certificate.CertificateId);
		p.Add("CertificateName", Certificate.CertificateName);
		p.Add("Description", Certificate.Description);
		
		p.Add("LastModifiedBy", Certificate.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Certificate_Update", p);
	}

	public async Task DeleteCertificate(int CertificateId, LogModel logModel)
	{
		ClearCache(CertificateCache);
		ClearCache(DistinctCertificateCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CertificateId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Certificate_Delete", p);
	}

	public async Task<List<CertificateModel>> Export()
	{
		return await _dataAccessHelper.QueryData<CertificateModel, dynamic>("USP_Certificate_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CertificateCache:
				var keys = _cache.Get<List<string>>(CertificateCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CertificateCache);
				}
				break;		
			case DistinctCertificateCache:
					_cache.Remove(DistinctCertificateCache);

				break;
			default:
				break;
		}
	}
	#endregion
}