using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantCertificationRepository : IApplicantCertificationRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantCertificationCache = "ApplicantCertificationData";
	private const string DistinctApplicantCertificationCache = "DistinctApplicantCertificationData";

	public ApplicantCertificationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantCertificationModel>> GetApplicantCertificationsByApplicantId(int ApplicantID)
	{
		return await _dataAccessHelper.QueryData<ApplicantCertificationModel, dynamic>("USP_ApplicantCertifications_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantCertificationModel> GetApplicantCertificationById(int ApplicantCertificationId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantCertificationModel, dynamic>("USP_ApplicantCertification_GetById", new { Id = ApplicantCertificationId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantCertification(ApplicantCertificationModel ApplicantCertification, LogModel logModel)
	{
		ClearCache(ApplicantCertificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantCertification.ApplicantID);
		p.Add("CertificateId", ApplicantCertification.CertificateId);
		p.Add("CertificateAuthorityId", ApplicantCertification.CertificateAuthorityId);
		p.Add("CertificateNumber", ApplicantCertification.CertificateNumber);
		p.Add("ImageUrl", ApplicantCertification.ImageUrl);
		p.Add("CountryID", ApplicantCertification.CountryID);
		p.Add("Year", ApplicantCertification.Year);

		p.Add("CreatedBy", ApplicantCertification.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCertification_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantCertification(ApplicantCertificationModel ApplicantCertification, LogModel logModel)
	{
		ClearCache(ApplicantCertificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantCertification.ApplicantCertificationId);
		p.Add("ApplicantID", ApplicantCertification.ApplicantID);
		p.Add("CertificateId", ApplicantCertification.CertificateId);
		p.Add("CertificateAuthorityId", ApplicantCertification.CertificateAuthorityId);
		p.Add("CertificateNumber", ApplicantCertification.CertificateNumber);
		p.Add("ImageUrl", ApplicantCertification.ImageUrl);
		p.Add("CountryID", ApplicantCertification.CountryID);
		p.Add("Year", ApplicantCertification.Year);

		p.Add("LastModifiedBy", ApplicantCertification.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCertification_Update", p);
	}


	public async Task DeleteApplicantCertification(int ApplicantCertificationId, LogModel logModel)
	{
		ClearCache(ApplicantCertificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantCertificationId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCertification_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantCertificationCache:
				var keys = _cache.Get<List<string>>(ApplicantCertificationCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantCertificationCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}