using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class CandidateGPTRepository : ICandidateGPTRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CandidateGPTCache = "CandidateGPTData";
	private const string DistinctCandidateGPTCache = "DistinctCandidateGPTData";

	public CandidateGPTRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<CandidateGPTModel>> GetCandidateGPTsByCandidateId(int CandidateID)
	{
	
		return await _dataAccessHelper.QueryData<CandidateGPTModel, dynamic>("USP_CandidateGPT_GetByCandidateId", new { CandidateID = CandidateID });
	}

	public async Task<CandidateGPTModel> GetCandidateGPTById(int CandidateGPTId)
	{
		return (await _dataAccessHelper.QueryData<CandidateGPTModel, dynamic>("USP_CandidateGPT_GetById", new { Id = CandidateGPTId })).FirstOrDefault();
	}



	public async Task<List<CandidateGPTModel>> GetTopPerformers()
	{
		var output = _cache.Get<List<CandidateGPTModel>>(DistinctCandidateGPTCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CandidateGPTModel, dynamic>("USP_TopPerformer", new { });
			_cache.Set(DistinctCandidateGPTCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}
	public async Task<int> InsertCandidateGPT(CandidateGPTModel CandidateGPT, LogModel logModel)
	{
		ClearCache(CandidateGPTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("CandidateID", CandidateGPT.CandidateID);
		//p.Add("PostingDate", CandidateGPT.PostingDate);
		p.Add("WT1st", CandidateGPT.WT1st);
		p.Add("WT2nd", CandidateGPT.WT2nd);
		p.Add("WT3rd", CandidateGPT.WT3rd);
		p.Add("WT4th", CandidateGPT.WT4th);
		p.Add("WT5th", CandidateGPT.WT5th);
		p.Add("WT6th", CandidateGPT.WT6th);
		p.Add("WTTotalMk", CandidateGPT.WTTotalMk);
		p.Add("WTTotalWt", CandidateGPT.WTTotalWt);

		p.Add("WH1st", CandidateGPT.WH1st);
		p.Add("WH2nd", CandidateGPT.WH2nd);
		p.Add("WH3rd", CandidateGPT.WH3rd);
		p.Add("WH4th", CandidateGPT.WH4th);
		p.Add("WHTotalMk", CandidateGPT.WHTotalMk);
		p.Add("WHTotalWt", CandidateGPT.WHTotalWt);

		p.Add("STX", CandidateGPT.STX);
		p.Add("STX2", CandidateGPT.STX2);
		p.Add("STX3", CandidateGPT.STX3);
		p.Add("STX4", CandidateGPT.STX4);
		p.Add("STX5", CandidateGPT.STX5);
		p.Add("STX6", CandidateGPT.STX6);
		p.Add("STX7", CandidateGPT.STX7);
		p.Add("STX8", CandidateGPT.STX8);
		p.Add("STX9", CandidateGPT.STX9);
		p.Add("STXTotalMk", CandidateGPT.STXTotalMk);
		p.Add("STXTotalWt", CandidateGPT.STXTotalWt);

		p.Add("PracParts", CandidateGPT.PracParts);
		p.Add("PracETS", CandidateGPT.PracETS);
		p.Add("PracCC", CandidateGPT.PracCC);
		p.Add("PracSalutingTest", CandidateGPT.PracSalutingTest);
		p.Add("PracTotalMk", CandidateGPT.PracTotalMk);
		p.Add("PracTotalWt", CandidateGPT.PracTotalWt);


		p.Add("FEWritten", CandidateGPT.FEWritten);
		p.Add("FEPrac", CandidateGPT.FEPrac);
		p.Add("FETotalMk", CandidateGPT.FETotalMk );
		p.Add("FETotalWt", CandidateGPT.FETotalWt);	
		
		p.Add("CEETotalMk", CandidateGPT.CEETotalMk);
		p.Add("CEETotalWt", CandidateGPT.CEETotalWt);
		
		
		p.Add("AdminGenAware", CandidateGPT.AdminGenAware);
		p.Add("AdminDecipline", CandidateGPT.AdminDecipline);
		p.Add("AdminTotalMk", CandidateGPT.AdminTotalMk);
		p.Add("AdminTotalWt", CandidateGPT.AdminTotalWt);
		
		p.Add("GrandTotalMk", CandidateGPT.GrandTotalMk);
		p.Add("GrandTotalWt", CandidateGPT.GrandTotalWt);
		p.Add("Remarks", CandidateGPT.Remarks);
		

		p.Add("CreatedBy", CandidateGPT.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateGPT_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCandidateGPT(CandidateGPTModel CandidateGPT, LogModel logModel)
	{
		ClearCache(CandidateGPTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateGPT.CandidateGPTId);
		p.Add("CandidateID", CandidateGPT.CandidateID);
		//p.Add("PostingDate", CandidateGPT.PostingDate);
		p.Add("WT1st", CandidateGPT.WT1st);
		p.Add("WT2nd", CandidateGPT.WT2nd);
		p.Add("WT3rd", CandidateGPT.WT3rd);
		p.Add("WT4th", CandidateGPT.WT4th);
		p.Add("WT5th", CandidateGPT.WT5th);
		p.Add("WT6th", CandidateGPT.WT6th);
		p.Add("WTTotalMk", CandidateGPT.WTTotalMk);
		p.Add("WTTotalWt", CandidateGPT.WTTotalWt);

		p.Add("WH1st", CandidateGPT.WH1st);
		p.Add("WH2nd", CandidateGPT.WH2nd);
		p.Add("WH3rd", CandidateGPT.WH3rd);
		p.Add("WH4th", CandidateGPT.WH4th);
		p.Add("WHTotalMk", CandidateGPT.WHTotalMk);
		p.Add("WHTotalWt", CandidateGPT.WHTotalWt);

		p.Add("STX", CandidateGPT.STX);
		p.Add("STX2", CandidateGPT.STX2);
		p.Add("STX3", CandidateGPT.STX3);
		p.Add("STX4", CandidateGPT.STX4);
		p.Add("STX5", CandidateGPT.STX5);
		p.Add("STX6", CandidateGPT.STX6);
		p.Add("STX7", CandidateGPT.STX7);
		p.Add("STX8", CandidateGPT.STX8);
		p.Add("STX9", CandidateGPT.STX9);
		p.Add("STXTotalMk", CandidateGPT.STXTotalMk);
		p.Add("STXTotalWt", CandidateGPT.STXTotalWt);

		p.Add("PracParts", CandidateGPT.PracParts);
		p.Add("PracETS", CandidateGPT.PracETS);
		p.Add("PracCC", CandidateGPT.PracCC);
		p.Add("PracSalutingTest", CandidateGPT.PracSalutingTest);
		p.Add("PracTotalMk", CandidateGPT.PracTotalMk);
		p.Add("PracTotalWt", CandidateGPT.PracTotalWt);


		p.Add("FEWritten", CandidateGPT.FEWritten);
		p.Add("FEPrac", CandidateGPT.FEPrac);
		p.Add("FETotalMk", CandidateGPT.FETotalMk);
		p.Add("FETotalWt", CandidateGPT.FETotalWt);


		p.Add("CEETotalMk", CandidateGPT.CEETotalMk);
		p.Add("CEETotalWt", CandidateGPT.CEETotalWt);

		p.Add("AdminGenAware", CandidateGPT.AdminGenAware);
		p.Add("AdminDecipline", CandidateGPT.AdminDecipline);
		p.Add("AdminTotalMk", CandidateGPT.AdminTotalMk);
		p.Add("AdminTotalWt", CandidateGPT.AdminTotalWt);

		p.Add("GrandTotalMk", CandidateGPT.GrandTotalMk);
		p.Add("GrandTotalWt", CandidateGPT.GrandTotalWt);
		p.Add("Remarks", CandidateGPT.Remarks);

		p.Add("LastModifiedBy", CandidateGPT.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateGPT_Update", p);
	}


	public async Task DeleteCandidateGPT(int CandidateGPTId, LogModel logModel)
	{
		ClearCache(CandidateGPTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", CandidateGPTId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_CandidateGPT_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CandidateGPTCache:
				var keys = _cache.Get<List<string>>(CandidateGPTCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CandidateGPTCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}