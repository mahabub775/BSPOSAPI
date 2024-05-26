using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantRETRepository : IApplicantRETRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantRETCache = "ApplicantRETData";
	private const string DistinctApplicantRETCache = "DistinctApplicantRETData";

	public ApplicantRETRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantRETModel>> GetApplicantRETsByApplicantId(int ApplicantID)
	{
		
		return await _dataAccessHelper.QueryData<ApplicantRETModel, dynamic>("USP_ApplicantRETs_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantRETModel> GetApplicantRETById(int ApplicantRETId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantRETModel, dynamic>("USP_ApplicantRET_GetById", new { Id = ApplicantRETId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantRET(ApplicantRETModel ApplicantRET, LogModel logModel)
	{
		ClearCache(ApplicantRETCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantRET.ApplicantID);
		p.Add("RETDate", ApplicantRET.RETDate);
		p.Add("Result", ApplicantRET.Result);
		p.Add("Mark", ApplicantRET.Mark);
		p.Add("Remarks", ApplicantRET.Remarks);

		p.Add("CreatedBy", ApplicantRET.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantRET_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantRET(ApplicantRETModel ApplicantRET, LogModel logModel)
	{
		ClearCache(ApplicantRETCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantRET.ApplicantRETId);
		p.Add("ApplicantID", ApplicantRET.ApplicantID);
		p.Add("RETDate", ApplicantRET.RETDate);
		p.Add("Result", ApplicantRET.Result);
		p.Add("Mark", ApplicantRET.Mark);
		p.Add("Remarks", ApplicantRET.Remarks);

		p.Add("LastModifiedBy", ApplicantRET.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantRET_Update", p);
	}


	public async Task DeleteApplicantRET(int ApplicantRETId, LogModel logModel)
	{
		ClearCache(ApplicantRETCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantRETId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantRET_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantRETCache:
				var keys = _cache.Get<List<string>>(ApplicantRETCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantRETCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}