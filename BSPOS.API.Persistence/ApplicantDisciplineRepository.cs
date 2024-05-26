using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantDisciplineRepository : IApplicantDisciplineRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantDisciplineCache = "ApplicantDisciplineData";
	private const string DistinctApplicantDisciplineCache = "DistinctApplicantDisciplineData";

	public ApplicantDisciplineRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantDisciplineModel>> GetApplicantDisciplinesByApplicantId(int ApplicantID)
	{
		
		return await _dataAccessHelper.QueryData<ApplicantDisciplineModel, dynamic>("USP_ApplicantDisciplines_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantDisciplineModel> GetApplicantDisciplineById(int ApplicantDisciplineId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantDisciplineModel, dynamic>("USP_ApplicantDiscipline_GetById", new { Id = ApplicantDisciplineId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantDiscipline(ApplicantDisciplineModel ApplicantDiscipline, LogModel logModel)
	{
		ClearCache(ApplicantDisciplineCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantDiscipline.ApplicantID);
		p.Add("BAASectionId", ApplicantDiscipline.BAASectionId);
		p.Add("DisciplineDate", ApplicantDiscipline.DisciplineDate);
		p.Add("PunishmentType", ApplicantDiscipline.PunishmentType);
		p.Add("Remarks", ApplicantDiscipline.Remarks);

		p.Add("CreatedBy", ApplicantDiscipline.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantDiscipline_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantDiscipline(ApplicantDisciplineModel ApplicantDiscipline, LogModel logModel)
	{
		ClearCache(ApplicantDisciplineCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantDiscipline.ApplicantDisciplineId);
		p.Add("ApplicantID", ApplicantDiscipline.ApplicantID);

		p.Add("BAASectionId", ApplicantDiscipline.BAASectionId);
		p.Add("DisciplineDate", ApplicantDiscipline.DisciplineDate);
		p.Add("PunishmentType", ApplicantDiscipline.PunishmentType);
		p.Add("Remarks", ApplicantDiscipline.Remarks);


		p.Add("LastModifiedBy", ApplicantDiscipline.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantDiscipline_Update", p);
	}


	public async Task DeleteApplicantDiscipline(int ApplicantDisciplineId, LogModel logModel)
	{
		ClearCache(ApplicantDisciplineCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantDisciplineId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantDiscipline_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantDisciplineCache:
				var keys = _cache.Get<List<string>>(ApplicantDisciplineCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantDisciplineCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}