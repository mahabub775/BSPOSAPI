using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantRoleMappingRepository : IApplicantRoleMappingRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantRoleMappingCache = "ApplicantRoleMappingData";
	private const string DistinctApplicantRoleMappingCache = "DistinctApplicantRoleMappingData";

	public ApplicantRoleMappingRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<ApplicantRoleMappingModel>> GetApplicantRoleMappings(int pageNumber, int BrigadeID, int UnitID, int CompanyID, int PlatoonID)
	{
		PaginatedListModel<ApplicantRoleMappingModel> output = _cache.Get<PaginatedListModel<ApplicantRoleMappingModel>>(ApplicantRoleMappingCache +pageNumber+ BrigadeID+UnitID+CompanyID+PlatoonID.ToString());

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("BrigadeID", BrigadeID);
			p.Add("UnitID", UnitID);
			p.Add("CompanyID", CompanyID);
			p.Add("PlatoonID", PlatoonID);
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<ApplicantRoleMappingModel, dynamic>("USP_ApplicantRoleMapping_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<ApplicantRoleMappingModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(ApplicantRoleMappingCache + pageNumber+BrigadeID+UnitID+CompanyID+PlatoonID, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(ApplicantRoleMappingCache);
			if (keys is null)
				keys = new List<string> { ApplicantRoleMappingCache + pageNumber + BrigadeID + UnitID + CompanyID + PlatoonID };
			else
				keys.Add(ApplicantRoleMappingCache + pageNumber + BrigadeID + UnitID + CompanyID + PlatoonID);
			_cache.Set(ApplicantRoleMappingCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}


	public async Task<ApplicantRoleMappingModel> GetApplicantRoleMappingById(int ApplicantRoleMappingId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantRoleMappingModel, dynamic>("USP_ApplicantRoleMapping_GetById", new { Id = ApplicantRoleMappingId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantRoleMapping(ApplicantRoleMappingModel ApplicantRoleMapping, LogModel logModel)
	{
		ClearCache(ApplicantRoleMappingCache);
		ClearCache(DistinctApplicantRoleMappingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("BrigadeId", ApplicantRoleMapping.BrigadeID);
		p.Add("UnitId", ApplicantRoleMapping.UnitID);
		p.Add("CompanyId", ApplicantRoleMapping.CompanyID);
		p.Add("PlatoonId", ApplicantRoleMapping.PlatoonID);
		p.Add("CreatedBy", ApplicantRoleMapping.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantRoleMapping_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantRoleMapping(ApplicantRoleMappingModel ApplicantRoleMapping, LogModel logModel)
	{
		ClearCache(ApplicantRoleMappingCache);
		ClearCache(DistinctApplicantRoleMappingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("ApplicantRoleMappingId", ApplicantRoleMapping.ApplicantRoleMappingId);
		p.Add("BrigadeId", ApplicantRoleMapping.BrigadeID);
		p.Add("UnitId", ApplicantRoleMapping.UnitID);
		p.Add("CompanyId", ApplicantRoleMapping.CompanyID);
		p.Add("PlatoonId", ApplicantRoleMapping.PlatoonID);

		p.Add("LastModifiedBy", ApplicantRoleMapping.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantRoleMapping_Update", p);
	}

	public async Task DeleteApplicantRoleMapping(int ApplicantRoleMappingId, LogModel logModel)
	{
		ClearCache(ApplicantRoleMappingCache);
		ClearCache(DistinctApplicantRoleMappingCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantRoleMappingId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantRoleMapping_Delete", p);
	}

	public async Task<List<ApplicantRoleMappingModel>> Export(int BrigadeId, int UnitId, int CompanyId, int PlatoonId)
	{
		DynamicParameters p = new DynamicParameters();
		
		p.Add("BrigadeID", BrigadeId);
		p.Add("UnitID", UnitId);
		p.Add("CompanyID", CompanyId);
		p.Add("PlatoonID", PlatoonId);

		return await _dataAccessHelper.QueryData<ApplicantRoleMappingModel, dynamic>("USP_ApplicantRoleMapping_Export", p);
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantRoleMappingCache:
				var keys = _cache.Get<List<string>>(ApplicantRoleMappingCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantRoleMappingCache);
				}
				break;
			
			case DistinctApplicantRoleMappingCache:
						_cache.Remove(DistinctApplicantRoleMappingCache);
			
				break;
			default:
				break;
		}
	}
	#endregion
}