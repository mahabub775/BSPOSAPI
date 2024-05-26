using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantIPFTRepository : IApplicantIPFTRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantIPFTCache = "ApplicantIPFTData";
	private const string DistinctApplicantIPFTCache = "DistinctApplicantIPFTData";

	public ApplicantIPFTRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantIPFTModel>> GetApplicantIPFTsByApplicantId(int ApplicantID)
	{
		
		return await _dataAccessHelper.QueryData<ApplicantIPFTModel, dynamic>("USP_ApplicantIPFTs_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantIPFTModel> GetApplicantIPFTById(int ApplicantIPFTId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantIPFTModel, dynamic>("USP_ApplicantIPFT_GetById", new { Id = ApplicantIPFTId })).FirstOrDefault();
	}



	public async Task<int> InsertApplicantIPFT(ApplicantIPFTModel ApplicantIPFT, LogModel logModel)
	{
		ClearCache(ApplicantIPFTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantIPFT.ApplicantID);
		p.Add("BIAnnualId", ApplicantIPFT.BIAnnualId);
		p.Add("IPFTDate", ApplicantIPFT.IPFTDate);
		p.Add("Result", ApplicantIPFT.Result);
		p.Add("Attempt", ApplicantIPFT.Attempt);
		p.Add("Remarks", ApplicantIPFT.Remarks);

		p.Add("CreatedBy", ApplicantIPFT.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantIPFT_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantIPFT(ApplicantIPFTModel ApplicantIPFT, LogModel logModel)
	{
		ClearCache(ApplicantIPFTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantIPFT.ApplicantIPFTId);
		p.Add("ApplicantID", ApplicantIPFT.ApplicantID);
		p.Add("BIAnnualId", ApplicantIPFT.BIAnnualId);
		p.Add("IPFTDate", ApplicantIPFT.IPFTDate);
		p.Add("Result", ApplicantIPFT.Result);
		p.Add("Attempt", ApplicantIPFT.Attempt);
		p.Add("Remarks", ApplicantIPFT.Remarks);

		p.Add("LastModifiedBy", ApplicantIPFT.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantIPFT_Update", p);
	}



	public async Task DeleteApplicantIPFT(int ApplicantIPFTId, LogModel logModel)
	{
		ClearCache(ApplicantIPFTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantIPFTId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantIPFT_Delete", p);
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantIPFTCache:
				var keys = _cache.Get<List<string>>(ApplicantIPFTCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantIPFTCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}