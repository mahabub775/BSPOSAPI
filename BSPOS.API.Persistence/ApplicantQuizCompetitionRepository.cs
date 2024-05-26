using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantQuizCompetitionRepository : IApplicantQuizCompetitionRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantQuizCompetitionCache = "ApplicantQuizCompetitionData";
	private const string DistinctApplicantQuizCompetitionCache = "DistinctApplicantQuizCompetitionData";

	public ApplicantQuizCompetitionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantQuizCompetitionModel>> GetApplicantQuizCompetitionsByApplicantId(int ApplicantID)
	{
		
		return await _dataAccessHelper.QueryData<ApplicantQuizCompetitionModel, dynamic>("USP_ApplicantQuizCompetitions_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantQuizCompetitionModel> GetApplicantQuizCompetitionById(int ApplicantQuizCompetitionId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantQuizCompetitionModel, dynamic>("USP_ApplicantQuizCompetition_GetById", new { Id = ApplicantQuizCompetitionId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantQuizCompetition(ApplicantQuizCompetitionModel ApplicantQuizCompetition, LogModel logModel)
	{
		ClearCache(ApplicantQuizCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantQuizCompetition.ApplicantID);
		p.Add("Name", ApplicantQuizCompetition.Name);
		p.Add("CompetitionDate", ApplicantQuizCompetition.CompetitionDate);
		p.Add("Number", ApplicantQuizCompetition.Number);
		p.Add("Remarks", ApplicantQuizCompetition.Remarks);

		p.Add("CreatedBy", ApplicantQuizCompetition.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantQuizCompetition_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantQuizCompetition(ApplicantQuizCompetitionModel ApplicantQuizCompetition, LogModel logModel)
	{
		ClearCache(ApplicantQuizCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantQuizCompetition.ApplicantQuizCompetitionId);
		p.Add("ApplicantID", ApplicantQuizCompetition.ApplicantID);
		p.Add("Name", ApplicantQuizCompetition.Name);
		p.Add("CompetitionDate", ApplicantQuizCompetition.CompetitionDate);
		p.Add("Number", ApplicantQuizCompetition.Number);
		p.Add("Remarks", ApplicantQuizCompetition.Remarks);

		p.Add("LastModifiedBy", ApplicantQuizCompetition.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantQuizCompetition_Update", p);
	}


	public async Task DeleteApplicantQuizCompetition(int ApplicantQuizCompetitionId, LogModel logModel)
	{
		ClearCache(ApplicantQuizCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantQuizCompetitionId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantQuizCompetition_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantQuizCompetitionCache:
				var keys = _cache.Get<List<string>>(ApplicantQuizCompetitionCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantQuizCompetitionCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}