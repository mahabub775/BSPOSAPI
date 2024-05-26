using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantCivilEducationRepository : IApplicantCivilEducationRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantCivilEducationCache = "ApplicantCivilEducationData";
	private const string DistinctApplicantCivilEducationCache = "DistinctApplicantCivilEducationData";

	public ApplicantCivilEducationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantCivilEducationModel>> GetApplicantCivilEducationsByApplicantId(int ApplicantID)
	{
		return await _dataAccessHelper.QueryData<ApplicantCivilEducationModel, dynamic>("USP_ApplicantCivilEducations_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantCivilEducationModel> GetApplicantCivilEducationById(int ApplicantCivilEducationId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantCivilEducationModel, dynamic>("USP_ApplicantCivilEducation_GetById", new { Id = ApplicantCivilEducationId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantCivilEducation(ApplicantCivilEducationModel ApplicantCivilEducation, LogModel logModel)
	{
		ClearCache(ApplicantCivilEducationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantCivilEducation.ApplicantID);
		p.Add("DegreeID", ApplicantCivilEducation.DegreeID);
		p.Add("InstitutionID", ApplicantCivilEducation.InstitutionID);
		p.Add("Result", ApplicantCivilEducation.Result);
		p.Add("YearOfPassing", ApplicantCivilEducation.YearOfPassing);
		p.Add("DurationID", ApplicantCivilEducation.DurationID);
		
		p.Add("CreatedBy", ApplicantCivilEducation.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCivilEducation_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantCivilEducation(ApplicantCivilEducationModel ApplicantCivilEducation, LogModel logModel)
	{
		ClearCache(ApplicantCivilEducationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantCivilEducation.ApplicantCivilEducationId);

		p.Add("ApplicantID", ApplicantCivilEducation.ApplicantID);
		p.Add("DegreeID", ApplicantCivilEducation.DegreeID);
		p.Add("InstitutionID", ApplicantCivilEducation.InstitutionID);
		p.Add("Result", ApplicantCivilEducation.Result);
		p.Add("YearOfPassing", ApplicantCivilEducation.YearOfPassing);
		p.Add("DurationID", ApplicantCivilEducation.DurationID);

		p.Add("LastModifiedBy", ApplicantCivilEducation.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCivilEducation_Update", p);
	}


	public async Task DeleteApplicantCivilEducation(int ApplicantCivilEducationId, LogModel logModel)
	{
		ClearCache(ApplicantCivilEducationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantCivilEducationId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCivilEducation_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantCivilEducationCache:
				var keys = _cache.Get<List<string>>(ApplicantCivilEducationCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantCivilEducationCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}