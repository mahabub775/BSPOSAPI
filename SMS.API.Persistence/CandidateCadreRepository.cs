using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateCadreRepository : ICandidateCadreRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateCadreCache = "CandidateCadreData";
	private const string DistinctCandidateCadreCache = "DistinctCandidateCadreData";

	public CandidateCadreRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateCadreModel>> GetCandidateCadresByCandidateId(int CandidateID)
	{
		
		return await _dataAccessHelper.QueryData<CandidateCadreModel, dynamic>("USP_CandidateCadres_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateCadreModel> GetCandidateCadreById(int CandidateCadreId)
	{
		return (await _dataAccessHelper.QueryData<CandidateCadreModel, dynamic>("USP_CandidateCadre_GetById", new { Id = CandidateCadreId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateCadre(CandidateCadreModel CandidateCadre, LogModel logModel)
	{
		ClearCache(CandidateCadreCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateCadre.CandidateID);
		p.Add("CourseID", CandidateCadre.CourseID);
		p.Add("Result", CandidateCadre.Result);
		p.Add("InstitutionID", CandidateCadre.InstitutionID);

		p.Add("CreatedBy", CandidateCadre.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCadre_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateCadre(CandidateCadreModel CandidateCadre, LogModel logModel)
	{
		ClearCache(CandidateCadreCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateCadre.CandidateCadreId);
		p.Add("CandidateID", CandidateCadre.CandidateID);
		p.Add("CourseID", CandidateCadre.CourseID);
		p.Add("Result", CandidateCadre.Result);
		p.Add("InstitutionID", CandidateCadre.InstitutionID);

		p.Add("LastModifiedBy", CandidateCadre.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCadre_Update", p);
	}


	public async Task DeleteCandidateCadre(int CandidateCadreId, LogModel logModel)
	{
		ClearCache(CandidateCadreCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateCadreId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateCadre_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateCadreCache:
				var keys = _cache.Get<List<string>>(CandidateCadreCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateCadreCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}