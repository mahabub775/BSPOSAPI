using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateQuizCompetitionRepository : ICandidateQuizCompetitionRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateQuizCompetitionCache = "CandidateQuizCompetitionData";
	private const string DistinctCandidateQuizCompetitionCache = "DistinctCandidateQuizCompetitionData";

	public CandidateQuizCompetitionRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateQuizCompetitionModel>> GetCandidateQuizCompetitionsByCandidateId(int CandidateID)
	{
		
		return await _dataAccessHelper.QueryData<CandidateQuizCompetitionModel, dynamic>("USP_CandidateQuizCompetitions_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateQuizCompetitionModel> GetCandidateQuizCompetitionById(int CandidateQuizCompetitionId)
	{
		return (await _dataAccessHelper.QueryData<CandidateQuizCompetitionModel, dynamic>("USP_CandidateQuizCompetition_GetById", new { Id = CandidateQuizCompetitionId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateQuizCompetition(CandidateQuizCompetitionModel CandidateQuizCompetition, LogModel logModel)
	{
		ClearCache(CandidateQuizCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateQuizCompetition.CandidateID);
		p.Add("Name", CandidateQuizCompetition.Name);
		p.Add("CompetitionDate", CandidateQuizCompetition.CompetitionDate);
		p.Add("Number", CandidateQuizCompetition.Number);
		p.Add("Remarks", CandidateQuizCompetition.Remarks);

		p.Add("CreatedBy", CandidateQuizCompetition.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateQuizCompetition_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateQuizCompetition(CandidateQuizCompetitionModel CandidateQuizCompetition, LogModel logModel)
	{
		ClearCache(CandidateQuizCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateQuizCompetition.CandidateQuizCompetitionId);
		p.Add("CandidateID", CandidateQuizCompetition.CandidateID);
		p.Add("Name", CandidateQuizCompetition.Name);
		p.Add("CompetitionDate", CandidateQuizCompetition.CompetitionDate);
		p.Add("Number", CandidateQuizCompetition.Number);
		p.Add("Remarks", CandidateQuizCompetition.Remarks);

		p.Add("LastModifiedBy", CandidateQuizCompetition.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateQuizCompetition_Update", p);
	}


	public async Task DeleteCandidateQuizCompetition(int CandidateQuizCompetitionId, LogModel logModel)
	{
		ClearCache(CandidateQuizCompetitionCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateQuizCompetitionId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateQuizCompetition_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateQuizCompetitionCache:
				var keys = _cache.Get<List<string>>(CandidateQuizCompetitionCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateQuizCompetitionCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}