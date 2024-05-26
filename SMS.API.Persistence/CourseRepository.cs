using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CourseRepository : ICourseRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CourseCache = "CourseData";
	private const string DistinctCourseCache = "DistinctCourseData";

	public CourseRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<CourseModel>> GetCourses(int pageNumber)
	{
		PaginatedListModel<CourseModel> output = _cache.Get<PaginatedListModel<CourseModel>>(CourseCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<CourseModel, dynamic>("USP_Course_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<CourseModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(CourseCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(CourseCache);
			if (keys is null)
				keys = new List<string> { CourseCache + pageNumber };
			else
				keys.Add(CourseCache + pageNumber);
			_cache.Set(CourseCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<CourseModel>> GetDistinctCourses()
	{
		var output = _cache.Get<List<CourseModel>>(DistinctCourseCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CourseModel, dynamic>("USP_Course_GetDistinct", new { });
			_cache.Set(DistinctCourseCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<CourseModel>> GetTopMilitaryCourseTaken()
	{
		var output = _cache.Get<List<CourseModel>>(DistinctCourseCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CourseModel, dynamic>("USP_TopMilitaryCourseTaken", new { });
			_cache.Set(DistinctCourseCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<CourseModel> GetCourseById(int CourseId)
	{
		return (await _dataAccessHelper.QueryData<CourseModel, dynamic>("USP_Course_GetById", new { Id = CourseId })).FirstOrDefault();
	}

	public async Task<CourseModel> GetCourseByName(string CourseName)
	{
		return (await _dataAccessHelper.QueryData<CourseModel, dynamic>("USP_Course_GetByName", new { Name = CourseName })).FirstOrDefault();
	}

	public async Task<int> InsertCourse(CourseModel Course, LogModel logModel)
	{
		ClearCache(CourseCache);
		ClearCache(DistinctCourseCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("CourseName", Course.CourseName);
		p.Add("Description", Course.Description);
		p.Add("CreatedBy", Course.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Course_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCourse(CourseModel Course, LogModel logModel)
	{
		ClearCache(CourseCache);
		ClearCache(DistinctCourseCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("CourseId", Course.CourseId);
		p.Add("CourseName", Course.CourseName);
		p.Add("Description", Course.Description);
		
		p.Add("LastModifiedBy", Course.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Course_Update", p);
	}

	public async Task DeleteCourse(int CourseId, LogModel logModel)
	{
		ClearCache(CourseCache);
		ClearCache(DistinctCourseCache);
		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CourseId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Course_Delete", p);
	}

	public async Task<List<CourseModel>> Export()
	{
		return await _dataAccessHelper.QueryData<CourseModel, dynamic>("USP_Course_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CourseCache:
				var keys = _cache.Get<List<string>>(CourseCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CourseCache);
				}
				break;
			case DistinctCourseCache:
				_cache.Remove(DistinctCourseCache);
				break;
			default:
				break;
		}
	}
	#endregion
}