using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateRETRepository : ICandidateRETRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateRETCache = "CandidateRETData";
	private const string DistinctCandidateRETCache = "DistinctCandidateRETData";

	public CandidateRETRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateRETModel>> GetCandidateRETsByCandidateId(int CandidateID)
	{
		
		return await _dataAccessHelper.QueryData<CandidateRETModel, dynamic>("USP_CandidateRETs_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateRETModel> GetCandidateRETById(int CandidateRETId)
	{
		return (await _dataAccessHelper.QueryData<CandidateRETModel, dynamic>("USP_CandidateRET_GetById", new { Id = CandidateRETId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateRET(CandidateRETModel CandidateRET, LogModel logModel)
	{
		ClearCache(CandidateRETCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateRET.CandidateID);
		p.Add("RETDate", CandidateRET.RETDate);
		p.Add("Result", CandidateRET.Result);
		p.Add("Mark", CandidateRET.Mark);
		p.Add("Remarks", CandidateRET.Remarks);

		p.Add("CreatedBy", CandidateRET.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateRET_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateRET(CandidateRETModel CandidateRET, LogModel logModel)
	{
		ClearCache(CandidateRETCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateRET.CandidateRETId);
		p.Add("CandidateID", CandidateRET.CandidateID);
		p.Add("RETDate", CandidateRET.RETDate);
		p.Add("Result", CandidateRET.Result);
		p.Add("Mark", CandidateRET.Mark);
		p.Add("Remarks", CandidateRET.Remarks);

		p.Add("LastModifiedBy", CandidateRET.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateRET_Update", p);
	}


	public async Task DeleteCandidateRET(int CandidateRETId, LogModel logModel)
	{
		ClearCache(CandidateRETCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateRETId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateRET_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateRETCache:
				var keys = _cache.Get<List<string>>(CandidateRETCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateRETCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}