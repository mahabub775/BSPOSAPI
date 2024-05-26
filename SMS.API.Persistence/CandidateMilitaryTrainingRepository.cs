using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateMilitaryTrainingRepository : ICandidateMilitaryTrainingRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateMilitaryTrainingCache = "CandidateMilitaryTrainingData";
	private const string DistinctCandidateMilitaryTrainingCache = "DistinctCandidateMilitaryTrainingData";

	public CandidateMilitaryTrainingRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateMilitaryTrainingModel>> GetCandidateMilitaryTrainingsByCandidateId(int CandidateID)
	{
		return await _dataAccessHelper.QueryData<CandidateMilitaryTrainingModel, dynamic>("USP_CandidateMilitaryTrainings_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateMilitaryTrainingModel> GetCandidateMilitaryTrainingById(int CandidateMilitaryTrainingId)
	{
		return (await _dataAccessHelper.QueryData<CandidateMilitaryTrainingModel, dynamic>("USP_CandidateMilitaryTraining_GetById", new { Id = CandidateMilitaryTrainingId })).FirstOrDefault();
	}

	public async Task<int> InsertCandidateMilitaryTraining(CandidateMilitaryTrainingModel CandidateMilitaryTraining, LogModel logModel)
	{
		ClearCache(CandidateMilitaryTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateMilitaryTraining.CandidateID);
		p.Add("TrainingID", CandidateMilitaryTraining.TrainingID);
		p.Add("DurationID", CandidateMilitaryTraining.DurationID);
		p.Add("Result", CandidateMilitaryTraining.Result);
		p.Add("YearOfTraining", CandidateMilitaryTraining.YearOfTraining);

		p.Add("CreatedBy", CandidateMilitaryTraining.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateMilitaryTraining_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateMilitaryTraining(CandidateMilitaryTrainingModel CandidateMilitaryTraining, LogModel logModel)
	{
		ClearCache(CandidateMilitaryTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateMilitaryTraining.CandidateMilitaryTrainingId);
		
		p.Add("CandidateID", CandidateMilitaryTraining.CandidateID);
		p.Add("TrainingID", CandidateMilitaryTraining.TrainingID);
		p.Add("DurationID", CandidateMilitaryTraining.DurationID);
		p.Add("Result", CandidateMilitaryTraining.Result);
		p.Add("YearOfTraining", CandidateMilitaryTraining.YearOfTraining);

		p.Add("LastModifiedBy", CandidateMilitaryTraining.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateMilitaryTraining_Update", p);
	}


	public async Task DeleteCandidateMilitaryTraining(int CandidateMilitaryTrainingId, LogModel logModel)
	{
		ClearCache(CandidateMilitaryTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateMilitaryTrainingId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateMilitaryTraining_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateMilitaryTrainingCache:
				var keys = _cache.Get<List<string>>(CandidateMilitaryTrainingCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateMilitaryTrainingCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}