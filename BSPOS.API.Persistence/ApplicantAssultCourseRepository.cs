using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantAssultCourseRepository : IApplicantAssultCourseRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantAssultCourseCache = "ApplicantAssultCourseData";
	private const string DistinctApplicantAssultCourseCache = "DistinctApplicantAssultCourseData";

	public ApplicantAssultCourseRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantAssultCourseModel>> GetApplicantAssultCoursesByApplicantId(int ApplicantID)
	{
		
		return await _dataAccessHelper.QueryData<ApplicantAssultCourseModel, dynamic>("USP_ApplicantAssultCourses_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantAssultCourseModel> GetApplicantAssultCourseById(int ApplicantAssultCourseId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantAssultCourseModel, dynamic>("USP_ApplicantAssultCourse_GetById", new { Id = ApplicantAssultCourseId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantAssultCourse(ApplicantAssultCourseModel ApplicantAssultCourse, LogModel logModel)
	{
		ClearCache(ApplicantAssultCourseCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantAssultCourse.ApplicantID);
		p.Add("CourseDate", ApplicantAssultCourse.CourseDate);
		p.Add("CourseTime", ApplicantAssultCourse.CourseTime);
		p.Add("Mark", ApplicantAssultCourse.Mark);
		p.Add("Remarks", ApplicantAssultCourse.Remarks);

		p.Add("CreatedBy", ApplicantAssultCourse.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantAssultCourse_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantAssultCourse(ApplicantAssultCourseModel ApplicantAssultCourse, LogModel logModel)
	{
		ClearCache(ApplicantAssultCourseCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantAssultCourse.ApplicantAssultCourseId);
		p.Add("ApplicantID", ApplicantAssultCourse.ApplicantID);
		p.Add("CourseDate", ApplicantAssultCourse.CourseDate);
		p.Add("CourseTime", ApplicantAssultCourse.CourseTime);
		p.Add("Mark", ApplicantAssultCourse.Mark);
		p.Add("Remarks", ApplicantAssultCourse.Remarks);

		p.Add("LastModifiedBy", ApplicantAssultCourse.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantAssultCourse_Update", p);
	}

	public async Task DeleteApplicantAssultCourse(int ApplicantAssultCourseId, LogModel logModel)
	{
		ClearCache(ApplicantAssultCourseCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantAssultCourseId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantAssultCourse_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantAssultCourseCache:
				var keys = _cache.Get<List<string>>(ApplicantAssultCourseCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantAssultCourseCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}