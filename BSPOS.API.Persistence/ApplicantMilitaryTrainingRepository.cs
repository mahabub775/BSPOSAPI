using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantMilitaryTrainingRepository : IApplicantMilitaryTrainingRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantMilitaryTrainingCache = "ApplicantMilitaryTrainingData";
	private const string DistinctApplicantMilitaryTrainingCache = "DistinctApplicantMilitaryTrainingData";

	public ApplicantMilitaryTrainingRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantMilitaryTrainingModel>> GetApplicantMilitaryTrainingsByApplicantId(int ApplicantID)
	{
		return await _dataAccessHelper.QueryData<ApplicantMilitaryTrainingModel, dynamic>("USP_ApplicantMilitaryTrainings_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantMilitaryTrainingModel> GetApplicantMilitaryTrainingById(int ApplicantMilitaryTrainingId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantMilitaryTrainingModel, dynamic>("USP_ApplicantMilitaryTraining_GetById", new { Id = ApplicantMilitaryTrainingId })).FirstOrDefault();
	}

	public async Task<int> InsertApplicantMilitaryTraining(ApplicantMilitaryTrainingModel ApplicantMilitaryTraining, LogModel logModel)
	{
		ClearCache(ApplicantMilitaryTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantMilitaryTraining.ApplicantID);
		p.Add("TrainingID", ApplicantMilitaryTraining.TrainingID);
		p.Add("DurationID", ApplicantMilitaryTraining.DurationID);
		p.Add("Result", ApplicantMilitaryTraining.Result);
		p.Add("YearOfTraining", ApplicantMilitaryTraining.YearOfTraining);

		p.Add("CreatedBy", ApplicantMilitaryTraining.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantMilitaryTraining_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantMilitaryTraining(ApplicantMilitaryTrainingModel ApplicantMilitaryTraining, LogModel logModel)
	{
		ClearCache(ApplicantMilitaryTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantMilitaryTraining.ApplicantMilitaryTrainingId);
		
		p.Add("ApplicantID", ApplicantMilitaryTraining.ApplicantID);
		p.Add("TrainingID", ApplicantMilitaryTraining.TrainingID);
		p.Add("DurationID", ApplicantMilitaryTraining.DurationID);
		p.Add("Result", ApplicantMilitaryTraining.Result);
		p.Add("YearOfTraining", ApplicantMilitaryTraining.YearOfTraining);

		p.Add("LastModifiedBy", ApplicantMilitaryTraining.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantMilitaryTraining_Update", p);
	}


	public async Task DeleteApplicantMilitaryTraining(int ApplicantMilitaryTrainingId, LogModel logModel)
	{
		ClearCache(ApplicantMilitaryTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantMilitaryTrainingId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantMilitaryTraining_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantMilitaryTrainingCache:
				var keys = _cache.Get<List<string>>(ApplicantMilitaryTrainingCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantMilitaryTrainingCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}