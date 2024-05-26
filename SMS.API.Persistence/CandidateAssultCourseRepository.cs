using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateAssultCourseRepository : ICandidateAssultCourseRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateAssultCourseCache = "CandidateAssultCourseData";
	private const string DistinctCandidateAssultCourseCache = "DistinctCandidateAssultCourseData";

	public CandidateAssultCourseRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateAssultCourseModel>> GetCandidateAssultCoursesByCandidateId(int CandidateID)
	{
		
		return await _dataAccessHelper.QueryData<CandidateAssultCourseModel, dynamic>("USP_CandidateAssultCourses_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateAssultCourseModel> GetCandidateAssultCourseById(int CandidateAssultCourseId)
	{
		return (await _dataAccessHelper.QueryData<CandidateAssultCourseModel, dynamic>("USP_CandidateAssultCourse_GetById", new { Id = CandidateAssultCourseId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateAssultCourse(CandidateAssultCourseModel CandidateAssultCourse, LogModel logModel)
	{
		ClearCache(CandidateAssultCourseCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateAssultCourse.CandidateID);
		p.Add("CourseDate", CandidateAssultCourse.CourseDate);
		p.Add("CourseTime", CandidateAssultCourse.CourseTime);
		p.Add("Mark", CandidateAssultCourse.Mark);
		p.Add("Remarks", CandidateAssultCourse.Remarks);

		p.Add("CreatedBy", CandidateAssultCourse.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateAssultCourse_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateAssultCourse(CandidateAssultCourseModel CandidateAssultCourse, LogModel logModel)
	{
		ClearCache(CandidateAssultCourseCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateAssultCourse.CandidateAssultCourseId);
		p.Add("CandidateID", CandidateAssultCourse.CandidateID);
		p.Add("CourseDate", CandidateAssultCourse.CourseDate);
		p.Add("CourseTime", CandidateAssultCourse.CourseTime);
		p.Add("Mark", CandidateAssultCourse.Mark);
		p.Add("Remarks", CandidateAssultCourse.Remarks);

		p.Add("LastModifiedBy", CandidateAssultCourse.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateAssultCourse_Update", p);
	}

	public async Task DeleteCandidateAssultCourse(int CandidateAssultCourseId, LogModel logModel)
	{
		ClearCache(CandidateAssultCourseCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateAssultCourseId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateAssultCourse_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateAssultCourseCache:
				var keys = _cache.Get<List<string>>(CandidateAssultCourseCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateAssultCourseCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}