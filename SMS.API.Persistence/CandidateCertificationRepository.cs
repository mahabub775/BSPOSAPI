using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateCertificationRepository : ICandidateCertificationRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateCertificationCache = "CandidateCertificationData";
	private const string DistinctCandidateCertificationCache = "DistinctCandidateCertificationData";

	public CandidateCertificationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateCertificationModel>> GetCandidateCertificationsByCandidateId(int CandidateID)
	{
		return await _dataAccessHelper.QueryData<CandidateCertificationModel, dynamic>("USP_CandidateCertifications_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateCertificationModel> GetCandidateCertificationById(int CandidateCertificationId)
	{
		return (await _dataAccessHelper.QueryData<CandidateCertificationModel, dynamic>("USP_CandidateCertification_GetById", new { Id = CandidateCertificationId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateCertification(CandidateCertificationModel CandidateCertification, LogModel logModel)
	{
		ClearCache(CandidateCertificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateCertification.CandidateID);
		p.Add("CertificateId", CandidateCertification.CertificateId);
		p.Add("CertificateAuthorityId", CandidateCertification.CertificateAuthorityId);
		p.Add("CertificateNumber", CandidateCertification.CertificateNumber);
		p.Add("ImageUrl", CandidateCertification.ImageUrl);
		p.Add("CountryID", CandidateCertification.CountryID);
		p.Add("Year", CandidateCertification.Year);

		p.Add("CreatedBy", CandidateCertification.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCertification_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateCertification(CandidateCertificationModel CandidateCertification, LogModel logModel)
	{
		ClearCache(CandidateCertificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateCertification.CandidateCertificationId);
		p.Add("CandidateID", CandidateCertification.CandidateID);
		p.Add("CertificateId", CandidateCertification.CertificateId);
		p.Add("CertificateAuthorityId", CandidateCertification.CertificateAuthorityId);
		p.Add("CertificateNumber", CandidateCertification.CertificateNumber);
		p.Add("ImageUrl", CandidateCertification.ImageUrl);
		p.Add("CountryID", CandidateCertification.CountryID);
		p.Add("Year", CandidateCertification.Year);

		p.Add("LastModifiedBy", CandidateCertification.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCertification_Update", p);
	}


	public async Task DeleteCandidateCertification(int CandidateCertificationId, LogModel logModel)
	{
		ClearCache(CandidateCertificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateCertificationId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCertification_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateCertificationCache:
				var keys = _cache.Get<List<string>>(CandidateCertificationCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateCertificationCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}