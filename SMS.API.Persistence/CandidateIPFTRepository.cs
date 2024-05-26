using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateIPFTRepository : ICandidateIPFTRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateIPFTCache = "CandidateIPFTData";
	private const string DistinctCandidateIPFTCache = "DistinctCandidateIPFTData";

	public CandidateIPFTRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateIPFTModel>> GetCandidateIPFTsByCandidateId(int CandidateID)
	{
		
		return await _dataAccessHelper.QueryData<CandidateIPFTModel, dynamic>("USP_CandidateIPFTs_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateIPFTModel> GetCandidateIPFTById(int CandidateIPFTId)
	{
		return (await _dataAccessHelper.QueryData<CandidateIPFTModel, dynamic>("USP_CandidateIPFT_GetById", new { Id = CandidateIPFTId })).FirstOrDefault();
	}



	public async Task<int> InsertCandidateIPFT(CandidateIPFTModel CandidateIPFT, LogModel logModel)
	{
		ClearCache(CandidateIPFTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateIPFT.CandidateID);
		p.Add("BIAnnualId", CandidateIPFT.BIAnnualId);
		p.Add("IPFTDate", CandidateIPFT.IPFTDate);
		p.Add("Result", CandidateIPFT.Result);
		p.Add("Attempt", CandidateIPFT.Attempt);
		p.Add("Remarks", CandidateIPFT.Remarks);

		p.Add("CreatedBy", CandidateIPFT.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateIPFT_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateIPFT(CandidateIPFTModel CandidateIPFT, LogModel logModel)
	{
		ClearCache(CandidateIPFTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateIPFT.CandidateIPFTId);
		p.Add("CandidateID", CandidateIPFT.CandidateID);
		p.Add("BIAnnualId", CandidateIPFT.BIAnnualId);
		p.Add("IPFTDate", CandidateIPFT.IPFTDate);
		p.Add("Result", CandidateIPFT.Result);
		p.Add("Attempt", CandidateIPFT.Attempt);
		p.Add("Remarks", CandidateIPFT.Remarks);

		p.Add("LastModifiedBy", CandidateIPFT.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateIPFT_Update", p);
	}



	public async Task DeleteCandidateIPFT(int CandidateIPFTId, LogModel logModel)
	{
		ClearCache(CandidateIPFTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateIPFTId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateIPFT_Delete", p);
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateIPFTCache:
				var keys = _cache.Get<List<string>>(CandidateIPFTCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateIPFTCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}