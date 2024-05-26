using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateAddlQualificationRepository : ICandidateAddlQualificationRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateAddlQualificationCache = "CandidateAddlQualificationData";
	private const string DistinctCandidateAddlQualificationCache = "DistinctCandidateAddlQualificationData";

	public CandidateAddlQualificationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateAddlQualificationModel>> GetCandidateAddlQualificationsByCandidateId(int CandidateID)
	{
		
		return await _dataAccessHelper.QueryData<CandidateAddlQualificationModel, dynamic>("USP_CandidateAddlQualifications_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateAddlQualificationModel> GetCandidateAddlQualificationById(int CandidateAddlQualificationId)
	{
		return (await _dataAccessHelper.QueryData<CandidateAddlQualificationModel, dynamic>("USP_CandidateAddlQualification_GetById", new { Id = CandidateAddlQualificationId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateAddlQualification(CandidateAddlQualificationModel CandidateAddlQualification, LogModel logModel)
	{
		ClearCache(CandidateAddlQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateAddlQualification.CandidateID);
		p.Add("QualificationID", CandidateAddlQualification.QualificationID);
		p.Add("Description", CandidateAddlQualification.Description);
		p.Add("ImageUrl", CandidateAddlQualification.ImageUrl);

		p.Add("CreatedBy", CandidateAddlQualification.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateAddlQualification_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateAddlQualification(CandidateAddlQualificationModel CandidateAddlQualification, LogModel logModel)
	{
		ClearCache(CandidateAddlQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateAddlQualification.CandidateAddlQualificationId);
		p.Add("CandidateID", CandidateAddlQualification.CandidateID);
		p.Add("QualificationID", CandidateAddlQualification.QualificationID);
		p.Add("Description", CandidateAddlQualification.Description);
		p.Add("ImageUrl", CandidateAddlQualification.ImageUrl);

		p.Add("LastModifiedBy", CandidateAddlQualification.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateAddlQualification_Update", p);
	}

	public async Task DeleteCandidateAddlQualification(int CandidateAddlQualificationId, LogModel logModel)
	{
		ClearCache(CandidateAddlQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateAddlQualificationId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateAddlQualification_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateAddlQualificationCache:
				var keys = _cache.Get<List<string>>(CandidateAddlQualificationCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateAddlQualificationCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}