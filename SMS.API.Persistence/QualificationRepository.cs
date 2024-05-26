using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class QualificationRepository : IQualificationRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string QualificationCache = "QualificationData";
	private const string DistinctQualificationCache = "DistinctQualificationData";

	public QualificationRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<QualificationModel>> GetQualifications(int pageNumber)
	{
		PaginatedListModel<QualificationModel> output = _cache.Get<PaginatedListModel<QualificationModel>>(QualificationCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<QualificationModel, dynamic>("USP_Qualification_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<QualificationModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(QualificationCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(QualificationCache);
			if (keys is null)
				keys = new List<string> { QualificationCache + pageNumber };
			else
				keys.Add(QualificationCache + pageNumber);
			_cache.Set(QualificationCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<QualificationModel>> GetDistinctQualifications()
	{
		var output = _cache.Get<List<QualificationModel>>(DistinctQualificationCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<QualificationModel, dynamic>("USP_Qualification_GetDistinct", new { });
			_cache.Set(DistinctQualificationCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<QualificationModel> GetQualificationById(int QualificationId)
	{
		return (await _dataAccessHelper.QueryData<QualificationModel, dynamic>("USP_Qualification_GetById", new { Id = QualificationId })).FirstOrDefault();
	}

	public async Task<QualificationModel> GetQualificationByName(string QualificationName)
	{
		return (await _dataAccessHelper.QueryData<QualificationModel, dynamic>("USP_Qualification_GetByName", new { Name = QualificationName })).FirstOrDefault();
	}

	public async Task<int> InsertQualification(QualificationModel Qualification, LogModel logModel)
	{
		ClearCache(QualificationCache);
		ClearCache(DistinctQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("QualificationName", Qualification.QualificationName);
		p.Add("Description", Qualification.Description);
		p.Add("CreatedBy", Qualification.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Qualification_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateQualification(QualificationModel Qualification, LogModel logModel)
	{
		ClearCache(QualificationCache);
		ClearCache(DistinctQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("QualificationId", Qualification.QualificationId);
		p.Add("QualificationName", Qualification.QualificationName);
		p.Add("Description", Qualification.Description);
		
		p.Add("LastModifiedBy", Qualification.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Qualification_Update", p);
	}

	public async Task DeleteQualification(int QualificationId, LogModel logModel)
	{
		ClearCache(QualificationCache);
		ClearCache(DistinctQualificationCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", QualificationId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Qualification_Delete", p);
	}

	public async Task<List<QualificationModel>> Export()
	{
		return await _dataAccessHelper.QueryData<QualificationModel, dynamic>("USP_Qualification_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case QualificationCache:
				var keys = _cache.Get<List<string>>(QualificationCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(QualificationCache);
				}
				break;
			case DistinctQualificationCache:
					_cache.Remove(DistinctQualificationCache);
				break;
			default:
				break;
		}
	}
	#endregion
}