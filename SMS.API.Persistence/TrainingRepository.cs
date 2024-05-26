using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class TrainingRepository : ITrainingRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string TrainingCache = "TrainingData";
	private const string DistinctTrainingCache = "DistinctTrainingData";

	public TrainingRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<TrainingModel>> GetTrainings(int pageNumber)
	{
		PaginatedListModel<TrainingModel> output = _cache.Get<PaginatedListModel<TrainingModel>>(TrainingCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<TrainingModel, dynamic>("USP_Training_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<TrainingModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(TrainingCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(TrainingCache);
			if (keys is null)
				keys = new List<string> { TrainingCache + pageNumber };
			else
				keys.Add(TrainingCache + pageNumber);
			_cache.Set(TrainingCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<TrainingModel>> GetDistinctTrainings()
	{
		var output = _cache.Get<List<TrainingModel>>(DistinctTrainingCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<TrainingModel, dynamic>("USP_Training_GetDistinct", new { });
			_cache.Set(DistinctTrainingCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<TrainingModel> GetTrainingById(int TrainingId)
	{
		return (await _dataAccessHelper.QueryData<TrainingModel, dynamic>("USP_Training_GetById", new { Id = TrainingId })).FirstOrDefault();
	}

	public async Task<TrainingModel> GetTrainingByName(string TrainingName)
	{
		return (await _dataAccessHelper.QueryData<TrainingModel, dynamic>("USP_Training_GetByName", new { Name = TrainingName })).FirstOrDefault();
	}

	public async Task<int> InsertTraining(TrainingModel Training, LogModel logModel)
	{
		ClearCache(TrainingCache);
		ClearCache(DistinctTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("TrainingName", Training.TrainingName);
		p.Add("Description", Training.Description);
		p.Add("CreatedBy", Training.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Training_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateTraining(TrainingModel Training, LogModel logModel)
	{
		ClearCache(TrainingCache);
		ClearCache(DistinctTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("TrainingId", Training.TrainingId);
		p.Add("TrainingName", Training.TrainingName);
		p.Add("Description", Training.Description);
		
		p.Add("LastModifiedBy", Training.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Training_Update", p);
	}

	public async Task DeleteTraining(int TrainingId, LogModel logModel)
	{
		ClearCache(TrainingCache);
		ClearCache(DistinctTrainingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", TrainingId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Training_Delete", p);
	}

	public async Task<List<TrainingModel>> Export()
	{
		return await _dataAccessHelper.QueryData<TrainingModel, dynamic>("USP_Training_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case TrainingCache:
				var keys = _cache.Get<List<string>>(TrainingCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(TrainingCache);
				}
				break;		
			case DistinctTrainingCache:
					_cache.Remove(DistinctTrainingCache);
				break;
			default:
				break;
		}
	}
	#endregion
}