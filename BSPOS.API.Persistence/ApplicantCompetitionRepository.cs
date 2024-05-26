using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantCompetitionRepository : IApplicantCompetitionRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantCompetitionCache = "ApplicantCompetitionData";
	private const string DistinctApplicantCompetitionCache = "DistinctApplicantCompetitionData";

	public ApplicantCompetitionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantCompetitionModel>> GetApplicantCompetitionsByApplicantId(int ApplicantID)
	{
		
		return await _dataAccessHelper.QueryData<ApplicantCompetitionModel, dynamic>("USP_ApplicantCompetitions_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantCompetitionModel> GetApplicantCompetitionById(int ApplicantCompetitionId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantCompetitionModel, dynamic>("USP_ApplicantCompetition_GetById", new { Id = ApplicantCompetitionId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantCompetition(ApplicantCompetitionModel ApplicantCompetition, LogModel logModel)
	{
		ClearCache(ApplicantCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantCompetition.ApplicantID);
		p.Add("CompetitionDate", ApplicantCompetition.CompetitionDate);
		p.Add("Name", ApplicantCompetition.Name);
		p.Add("Team", ApplicantCompetition.Team);
		p.Add("Remarks", ApplicantCompetition.Remarks);

		p.Add("CreatedBy", ApplicantCompetition.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCompetition_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantCompetition(ApplicantCompetitionModel ApplicantCompetition, LogModel logModel)
	{
		ClearCache(ApplicantCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantCompetition.ApplicantCompetitionId);
		p.Add("ApplicantID", ApplicantCompetition.ApplicantID);
		p.Add("CompetitionDate", ApplicantCompetition.CompetitionDate);
		p.Add("Name", ApplicantCompetition.Name);
		p.Add("Team", ApplicantCompetition.Team);
		p.Add("Remarks", ApplicantCompetition.Remarks);

		p.Add("LastModifiedBy", ApplicantCompetition.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCompetition_Update", p);
	}


	public async Task DeleteApplicantCompetition(int ApplicantCompetitionId, LogModel logModel)
	{
		ClearCache(ApplicantCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantCompetitionId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCompetition_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantCompetitionCache:
				var keys = _cache.Get<List<string>>(ApplicantCompetitionCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantCompetitionCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}