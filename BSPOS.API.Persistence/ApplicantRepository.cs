using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.ComponentModel.Design;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantRepository : IApplicantRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantCache = "ApplicantData";
	private const string DistinctApplicantCache = "DistinctApplicantData";
	public ApplicantRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<ApplicantModel>> GetApplicants(int pageNumber, int BrigadeID, int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId, string SoldierUserId, string ArmyNo, string Name)
	{
		PaginatedListModel<ApplicantModel> output = _cache.Get<PaginatedListModel<ApplicantModel>>(ApplicantCache + pageNumber+ BrigadeID + UnitId+CompanyId+PlatoonId+TradeId+RankId+ SoldierUserId + ArmyNo+Name);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("BrigadeID", BrigadeID);
			p.Add("CompanyID", CompanyId);
			p.Add("PlatoonID", PlatoonId);
			p.Add("TradeID", TradeId);
			p.Add("UnitID", UnitId);
			p.Add("RankID", RankId);
			p.Add("SoldierUserId", SoldierUserId == "SoldierUserId" ? "": SoldierUserId);
			p.Add("ArmyNo", ArmyNo== "ArmyNo"?"":ArmyNo);
			p.Add("Name", Name== "Name"?"":Name);
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<ApplicantModel, dynamic>("USP_Applicant_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<ApplicantModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(ApplicantCache + pageNumber + BrigadeID + UnitId + CompanyId + PlatoonId + TradeId + RankId + SoldierUserId + ArmyNo + Name, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(ApplicantCache);
			if (keys is null)
				keys = new List<string> { ApplicantCache + pageNumber + BrigadeID + UnitId + CompanyId + PlatoonId + TradeId + RankId + SoldierUserId + ArmyNo + Name };
			else
				keys.Add(ApplicantCache + pageNumber + BrigadeID + UnitId + CompanyId + PlatoonId + TradeId + RankId + SoldierUserId + ArmyNo + Name);
			_cache.Set(ApplicantCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}
	public async Task<List<ApplicantModel>> GetGroupReport(int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId)
	{
			DynamicParameters p = new DynamicParameters();
			p.Add("CompanyID", CompanyId);
			p.Add("PlatoonID", PlatoonId);
			p.Add("TradeID", TradeId);
			p.Add("UnitID", UnitId);
			p.Add("RankID", RankId);


		return await _dataAccessHelper.QueryData<ApplicantModel, dynamic>("USP_GroupReport", p);
	
	}


	public async Task<ApplicantModel> GetApplicantById(int ApplicantId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantModel, dynamic>("USP_Applicant_GetById", new { Id = ApplicantId })).FirstOrDefault();
	}
	public async Task<ApplicantModel> GetApplicantByUserId(string UserId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantModel, dynamic>("USP_Applicant_GetByUserId", new { Id = UserId })).FirstOrDefault();
	}

	public async Task<ApplicantModel> GetApplicantByName(string categoryName)
	{
		return (await _dataAccessHelper.QueryData<ApplicantModel, dynamic>("USP_Applicant_GetByName", new { Name = categoryName })).FirstOrDefault();
	}

	public async Task<int> InsertApplicant(ApplicantModel Applicant, LogModel logModel)
	{
		ClearCache(ApplicantCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("Name", Applicant.Name);
		p.Add("RankID", Applicant.RankID);
		p.Add("TradeID", Applicant.TradeID);
		//p.Add("CompanyID", Applicant.CompanyID);
		//p.Add("UnitID", Applicant.UnitID);
		//p.Add("PlatoonID", Applicant.PlatoonID);
		//p.Add("BrigadeID", Applicant.BrigadeID);
		p.Add("UserId", Applicant.UserId);
		p.Add("ArmyNo", Applicant.ArmyNo);
		p.Add("Mobile", Applicant.Mobile);
		p.Add("Email", Applicant.Email);
		p.Add("ImageUrl", Applicant.ImageUrl);
		p.Add("PostedDate", Applicant.PostedDate);
		p.Add("Active", Applicant.Active);

		p.Add("CreatedBy", Applicant.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Applicant_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicant(ApplicantModel Applicant, LogModel logModel)
	{
		ClearCache(ApplicantCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("ApplicantId", Applicant.ApplicantId);


		p.Add("Name", Applicant.Name);
		p.Add("RankID", Applicant.RankID);
		p.Add("TradeID", Applicant.TradeID);
		//p.Add("BrigadeID", Applicant.BrigadeID);
		//p.Add("CompanyID", Applicant.CompanyID);
		//p.Add("UnitID", Applicant.UnitID);
		//p.Add("PlatoonID", Applicant.PlatoonID);
		p.Add("UserId", Applicant.UserId);
		p.Add("ArmyNo", Applicant.ArmyNo);
		p.Add("Mobile", Applicant.Mobile);
		p.Add("Email", Applicant.Email);
		p.Add("ImageUrl", Applicant.ImageUrl);
		p.Add("PostedDate", Applicant.PostedDate);

		p.Add("LastModifiedBy", Applicant.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Applicant_Update", p);
	}

	public async Task DeleteApplicant(int ApplicantId, LogModel logModel)
	{
		ClearCache(ApplicantCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Applicant_Delete", p);
	}

	public async Task<List<ApplicantModel>> Export(int BrigadeID, int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId, string SoldierUserId, string ArmyNo, string Name)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("BrigadeID", BrigadeID);
		p.Add("CompanyID", CompanyId);
		p.Add("PlatoonID", PlatoonId);
		p.Add("TradeID", TradeId);
		p.Add("UnitID", UnitId);
		p.Add("RankID", RankId);
		p.Add("SoldierUserId", SoldierUserId == "SoldierUserId" ? "" : SoldierUserId);
		p.Add("ArmyNo", ArmyNo == "ArmyNo" ? "" : ArmyNo);
		p.Add("Name", Name == "Name" ? "" : Name);

		//var result = await _dataAccessHelper.QueryData<ApplicantModel, dynamic>("USP_Applicant_GetAll", p);
		return await _dataAccessHelper.QueryData<ApplicantModel, dynamic>("USP_Applicant_Export", p);


	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantCache:
				var keys = _cache.Get<List<string>>(ApplicantCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}