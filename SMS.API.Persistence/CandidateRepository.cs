using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.ComponentModel.Design;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateRepository : ICandidateRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateCache = "CandidateData";
	private const string DistinctCandidateCache = "DistinctCandidateData";

	public CandidateRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<CandidateModel>> GetCandidates(int pageNumber, int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId, string ArmyNo, string Name)
	{
		PaginatedListModel<CandidateModel> output = _cache.Get<PaginatedListModel<CandidateModel>>(CandidateCache + pageNumber+UnitId+CompanyId+PlatoonId+TradeId+RankId+ArmyNo+Name);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("CompanyID", CompanyId);
			p.Add("PlatoonID", PlatoonId);
			p.Add("TradeID", TradeId);
			p.Add("UnitID", UnitId);
			p.Add("RankID", RankId);
			p.Add("ArmyNo", ArmyNo== "ArmyNo"?"":ArmyNo);
			p.Add("Name", Name== "Name"?"":Name);
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<CandidateModel, dynamic>("USP_Candidate_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<CandidateModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(CandidateCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(CandidateCache);
			if (keys is null)
				keys = new List<string> { CandidateCache + pageNumber };
			else
				keys.Add(CandidateCache + pageNumber);
			_cache.Set(CandidateCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}
	public async Task<List<CandidateModel>> GetGroupReport(int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId)
	{
			DynamicParameters p = new DynamicParameters();
			p.Add("CompanyID", CompanyId);
			p.Add("PlatoonID", PlatoonId);
			p.Add("TradeID", TradeId);
			p.Add("UnitID", UnitId);
			p.Add("RankID", RankId);


		return await _dataAccessHelper.QueryData<CandidateModel, dynamic>("USP_GroupReport", p);
	
	}


	public async Task<CandidateModel> GetCandidateById(int CandidateId)
	{
		return (await _dataAccessHelper.QueryData<CandidateModel, dynamic>("USP_Candidate_GetById", new { Id = CandidateId })).FirstOrDefault();
	}

	public async Task<CandidateModel> GetCandidateByName(string categoryName)
	{
		return (await _dataAccessHelper.QueryData<CandidateModel, dynamic>("USP_Candidate_GetByName", new { Name = categoryName })).FirstOrDefault();
	}

	public async Task<int> InsertCandidate(CandidateModel Candidate, LogModel logModel)
	{
		ClearCache(CandidateCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("CandidateName", Candidate.CandidateName);
		p.Add("RankID", Candidate.RankID);
		p.Add("TradeID", Candidate.TradeID);
		p.Add("CompanyID", Candidate.CompanyID);
		p.Add("UnitID", Candidate.UnitID);
		p.Add("PlatoonID", Candidate.PlatoonID);
		p.Add("UserId", Candidate.UserId);
		p.Add("BrigadeID", Candidate.BrigadeID);
		p.Add("ArmyNo", Candidate.ArmyNo);
		p.Add("Mobile", Candidate.Mobile);
		p.Add("Email", Candidate.Email);
		p.Add("ImageUrl", Candidate.ImageUrl);
		p.Add("PostedDate", Candidate.PostedDate);
		p.Add("Active", Candidate.Active);

		p.Add("CreatedBy", Candidate.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Candidate_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidate(CandidateModel Candidate, LogModel logModel)
	{
		ClearCache(CandidateCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("CandidateId", Candidate.CandidateId);


		p.Add("CandidateName", Candidate.CandidateName);
		p.Add("RankID", Candidate.RankID);
		p.Add("TradeID", Candidate.TradeID);
		p.Add("CompanyID", Candidate.CompanyID);
		p.Add("UnitID", Candidate.UnitID);
		p.Add("PlatoonID", Candidate.PlatoonID);
		p.Add("UserId", Candidate.UserId);
		p.Add("BrigadeID", Candidate.BrigadeID);
		p.Add("ArmyNo", Candidate.ArmyNo);
		p.Add("Mobile", Candidate.Mobile);
		p.Add("Email", Candidate.Email);
		p.Add("ImageUrl", Candidate.ImageUrl);
		p.Add("PostedDate", Candidate.PostedDate);

		p.Add("LastModifiedBy", Candidate.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Candidate_Update", p);
	}

	public async Task DeleteCandidate(int CandidateId, LogModel logModel)
	{
		ClearCache(CandidateCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Candidate_Delete", p);
	}

	public async Task<List<CandidateModel>> Export()
	{
		return await _dataAccessHelper.QueryData<CandidateModel, dynamic>("USP_Candidate_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateCache:
				var keys = _cache.Get<List<string>>(CandidateCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}