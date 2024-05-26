using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantAddlQualificationRepository : IApplicantAddlQualificationRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantAddlQualificationCache = "ApplicantAddlQualificationData";
	private const string DistinctApplicantAddlQualificationCache = "DistinctApplicantAddlQualificationData";

	public ApplicantAddlQualificationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantAddlQualificationModel>> GetApplicantAddlQualificationsByApplicantId(int ApplicantID)
	{
		
		return await _dataAccessHelper.QueryData<ApplicantAddlQualificationModel, dynamic>("USP_ApplicantAddlQualifications_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantAddlQualificationModel> GetApplicantAddlQualificationById(int ApplicantAddlQualificationId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantAddlQualificationModel, dynamic>("USP_ApplicantAddlQualification_GetById", new { Id = ApplicantAddlQualificationId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantAddlQualification(ApplicantAddlQualificationModel ApplicantAddlQualification, LogModel logModel)
	{
		ClearCache(ApplicantAddlQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantAddlQualification.ApplicantID);
		p.Add("QualificationID", ApplicantAddlQualification.QualificationID);
		p.Add("Description", ApplicantAddlQualification.Description);
		p.Add("ImageUrl", ApplicantAddlQualification.ImageUrl);

		p.Add("CreatedBy", ApplicantAddlQualification.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantAddlQualification_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantAddlQualification(ApplicantAddlQualificationModel ApplicantAddlQualification, LogModel logModel)
	{
		ClearCache(ApplicantAddlQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantAddlQualification.ApplicantAddlQualificationId);
		p.Add("ApplicantID", ApplicantAddlQualification.ApplicantID);
		p.Add("QualificationID", ApplicantAddlQualification.QualificationID);
		p.Add("Description", ApplicantAddlQualification.Description);
		p.Add("ImageUrl", ApplicantAddlQualification.ImageUrl);

		p.Add("LastModifiedBy", ApplicantAddlQualification.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantAddlQualification_Update", p);
	}

	public async Task DeleteApplicantAddlQualification(int ApplicantAddlQualificationId, LogModel logModel)
	{
		ClearCache(ApplicantAddlQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantAddlQualificationId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantAddlQualification_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantAddlQualificationCache:
				var keys = _cache.Get<List<string>>(ApplicantAddlQualificationCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantAddlQualificationCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}