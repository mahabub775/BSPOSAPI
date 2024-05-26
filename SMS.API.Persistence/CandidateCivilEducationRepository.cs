using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateCivilEducationRepository : ICandidateCivilEducationRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateCivilEducationCache = "CandidateCivilEducationData";
	private const string DistinctCandidateCivilEducationCache = "DistinctCandidateCivilEducationData";

	public CandidateCivilEducationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateCivilEducationModel>> GetCandidateCivilEducationsByCandidateId(int CandidateID)
	{
		return await _dataAccessHelper.QueryData<CandidateCivilEducationModel, dynamic>("USP_CandidateCivilEducations_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateCivilEducationModel> GetCandidateCivilEducationById(int CandidateCivilEducationId)
	{
		return (await _dataAccessHelper.QueryData<CandidateCivilEducationModel, dynamic>("USP_CandidateCivilEducation_GetById", new { Id = CandidateCivilEducationId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateCivilEducation(CandidateCivilEducationModel CandidateCivilEducation, LogModel logModel)
	{
		ClearCache(CandidateCivilEducationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateCivilEducation.CandidateID);
		p.Add("DegreeID", CandidateCivilEducation.DegreeID);
		p.Add("InstitutionID", CandidateCivilEducation.InstitutionID);
		p.Add("Result", CandidateCivilEducation.Result);
		p.Add("YearOfPassing", CandidateCivilEducation.YearOfPassing);
		p.Add("DurationID", CandidateCivilEducation.DurationID);
		
		p.Add("CreatedBy", CandidateCivilEducation.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCivilEducation_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateCivilEducation(CandidateCivilEducationModel CandidateCivilEducation, LogModel logModel)
	{
		ClearCache(CandidateCivilEducationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateCivilEducation.CandidateCivilEducationId);

		p.Add("CandidateID", CandidateCivilEducation.CandidateID);
		p.Add("DegreeID", CandidateCivilEducation.DegreeID);
		p.Add("InstitutionID", CandidateCivilEducation.InstitutionID);
		p.Add("Result", CandidateCivilEducation.Result);
		p.Add("YearOfPassing", CandidateCivilEducation.YearOfPassing);
		p.Add("DurationID", CandidateCivilEducation.DurationID);

		p.Add("LastModifiedBy", CandidateCivilEducation.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCivilEducation_Update", p);
	}


	public async Task DeleteCandidateCivilEducation(int CandidateCivilEducationId, LogModel logModel)
	{
		ClearCache(CandidateCivilEducationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateCivilEducationId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCivilEducation_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateCivilEducationCache:
				var keys = _cache.Get<List<string>>(CandidateCivilEducationCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateCivilEducationCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}