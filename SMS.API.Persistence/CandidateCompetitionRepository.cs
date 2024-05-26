using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateCompetitionRepository : ICandidateCompetitionRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateCompetitionCache = "CandidateCompetitionData";
	private const string DistinctCandidateCompetitionCache = "DistinctCandidateCompetitionData";

	public CandidateCompetitionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateCompetitionModel>> GetCandidateCompetitionsByCandidateId(int CandidateID)
	{
		
		return await _dataAccessHelper.QueryData<CandidateCompetitionModel, dynamic>("USP_CandidateCompetitions_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateCompetitionModel> GetCandidateCompetitionById(int CandidateCompetitionId)
	{
		return (await _dataAccessHelper.QueryData<CandidateCompetitionModel, dynamic>("USP_CandidateCompetition_GetById", new { Id = CandidateCompetitionId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateCompetition(CandidateCompetitionModel CandidateCompetition, LogModel logModel)
	{
		ClearCache(CandidateCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateCompetition.CandidateID);
		p.Add("CompetitionDate", CandidateCompetition.CompetitionDate);
		p.Add("Name", CandidateCompetition.Name);
		p.Add("Team", CandidateCompetition.Team);
		p.Add("Remarks", CandidateCompetition.Remarks);

		p.Add("CreatedBy", CandidateCompetition.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCompetition_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateCompetition(CandidateCompetitionModel CandidateCompetition, LogModel logModel)
	{
		ClearCache(CandidateCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateCompetition.CandidateCompetitionId);
		p.Add("CandidateID", CandidateCompetition.CandidateID);
		p.Add("CompetitionDate", CandidateCompetition.CompetitionDate);
		p.Add("Name", CandidateCompetition.Name);
		p.Add("Team", CandidateCompetition.Team);
		p.Add("Remarks", CandidateCompetition.Remarks);

		p.Add("LastModifiedBy", CandidateCompetition.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCompetition_Update", p);
	}


	public async Task DeleteCandidateCompetition(int CandidateCompetitionId, LogModel logModel)
	{
		ClearCache(CandidateCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateCompetitionId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCompetition_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateCompetitionCache:
				var keys = _cache.Get<List<string>>(CandidateCompetitionCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateCompetitionCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}