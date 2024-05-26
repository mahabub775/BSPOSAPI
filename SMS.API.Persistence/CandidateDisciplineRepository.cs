using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateDisciplineRepository : ICandidateDisciplineRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateDisciplineCache = "CandidateDisciplineData";
	private const string DistinctCandidateDisciplineCache = "DistinctCandidateDisciplineData";

	public CandidateDisciplineRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateDisciplineModel>> GetCandidateDisciplinesByCandidateId(int CandidateID)
	{
		
		return await _dataAccessHelper.QueryData<CandidateDisciplineModel, dynamic>("USP_CandidateDisciplines_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateDisciplineModel> GetCandidateDisciplineById(int CandidateDisciplineId)
	{
		return (await _dataAccessHelper.QueryData<CandidateDisciplineModel, dynamic>("USP_CandidateDiscipline_GetById", new { Id = CandidateDisciplineId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateDiscipline(CandidateDisciplineModel CandidateDiscipline, LogModel logModel)
	{
		ClearCache(CandidateDisciplineCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateDiscipline.CandidateID);
		p.Add("BAASectionId", CandidateDiscipline.BAASectionId);
		p.Add("DisciplineDate", CandidateDiscipline.DisciplineDate);
		p.Add("PunishmentType", CandidateDiscipline.PunishmentType);
		p.Add("Remarks", CandidateDiscipline.Remarks);

		p.Add("CreatedBy", CandidateDiscipline.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateDiscipline_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateDiscipline(CandidateDisciplineModel CandidateDiscipline, LogModel logModel)
	{
		ClearCache(CandidateDisciplineCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateDiscipline.CandidateDisciplineId);
		p.Add("CandidateID", CandidateDiscipline.CandidateID);

		p.Add("BAASectionId", CandidateDiscipline.BAASectionId);
		p.Add("DisciplineDate", CandidateDiscipline.DisciplineDate);
		p.Add("PunishmentType", CandidateDiscipline.PunishmentType);
		p.Add("Remarks", CandidateDiscipline.Remarks);


		p.Add("LastModifiedBy", CandidateDiscipline.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateDiscipline_Update", p);
	}


	public async Task DeleteCandidateDiscipline(int CandidateDisciplineId, LogModel logModel)
	{
		ClearCache(CandidateDisciplineCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateDisciplineId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateDiscipline_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateDisciplineCache:
				var keys = _cache.Get<List<string>>(CandidateDisciplineCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateDisciplineCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}