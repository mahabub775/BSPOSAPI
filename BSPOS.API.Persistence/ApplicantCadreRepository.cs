using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantCadreRepository : IApplicantCadreRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantCadreCache = "ApplicantCadreData";
	private const string DistinctApplicantCadreCache = "DistinctApplicantCadreData";

	public ApplicantCadreRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantCadreModel>> GetApplicantCadresByApplicantId(int ApplicantID)
	{
		
		return await _dataAccessHelper.QueryData<ApplicantCadreModel, dynamic>("USP_ApplicantCadres_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantCadreModel> GetApplicantCadreById(int ApplicantCadreId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantCadreModel, dynamic>("USP_ApplicantCadre_GetById", new { Id = ApplicantCadreId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantCadre(ApplicantCadreModel ApplicantCadre, LogModel logModel)
	{
		ClearCache(ApplicantCadreCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantCadre.ApplicantID);
		p.Add("CourseID", ApplicantCadre.CourseID);
		p.Add("Result", ApplicantCadre.Result);
		p.Add("InstitutionID", ApplicantCadre.InstitutionID);

		p.Add("CreatedBy", ApplicantCadre.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCadre_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantCadre(ApplicantCadreModel ApplicantCadre, LogModel logModel)
	{
		ClearCache(ApplicantCadreCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantCadre.ApplicantCadreId);
		p.Add("ApplicantID", ApplicantCadre.ApplicantID);
		p.Add("CourseID", ApplicantCadre.CourseID);
		p.Add("Result", ApplicantCadre.Result);
		p.Add("InstitutionID", ApplicantCadre.InstitutionID);

		p.Add("LastModifiedBy", ApplicantCadre.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCadre_Update", p);
	}


	public async Task DeleteApplicantCadre(int ApplicantCadreId, LogModel logModel)
	{
		ClearCache(ApplicantCadreCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantCadreId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantCadre_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantCadreCache:
				var keys = _cache.Get<List<string>>(ApplicantCadreCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantCadreCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}