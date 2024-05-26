using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SMS.Core.Contract.Persistence;
using SMS.Core.Model;
using System.Data;

namespace SMS.API.Persistence;

public class ApplicantGPTRepository : IApplicantGPTRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string ApplicantGPTCache = "ApplicantGPTData";
	private const string DistinctApplicantGPTCache = "DistinctApplicantGPTData";

	public ApplicantGPTRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"

	public async Task<List<ApplicantGPTModel>> GetApplicantGPTsByApplicantId(int ApplicantID)
	{
	
		return await _dataAccessHelper.QueryData<ApplicantGPTModel, dynamic>("USP_ApplicantGPT_GetByApplicantId", new { ApplicantID = ApplicantID });
	}

	public async Task<ApplicantGPTModel> GetApplicantGPTById(int ApplicantGPTId)
	{
		return (await _dataAccessHelper.QueryData<ApplicantGPTModel, dynamic>("USP_ApplicantGPT_GetById", new { Id = ApplicantGPTId })).FirstOrDefault();
	}



	public async Task<List<ApplicantGPTModel>> GetTopPerformers()
	{
		return(await _dataAccessHelper.QueryData<ApplicantGPTModel, dynamic>("USP_TopPerformer", new { }));	 
	}
	public async Task<int> InsertApplicantGPT(ApplicantGPTModel ApplicantGPT, LogModel logModel)
	{
		ClearCache(ApplicantGPTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		
		p.Add("ApplicantID", ApplicantGPT.ApplicantID);
		//p.Add("PostingDate", ApplicantGPT.PostingDate);
		p.Add("WT1st", ApplicantGPT.WT1st);
		p.Add("WT2nd", ApplicantGPT.WT2nd);
		p.Add("WT3rd", ApplicantGPT.WT3rd);
		p.Add("WT4th", ApplicantGPT.WT4th);
		p.Add("WT5th", ApplicantGPT.WT5th);
		p.Add("WT6th", ApplicantGPT.WT6th);
		p.Add("WTTotalMk", ApplicantGPT.WTTotalMk);
		p.Add("WTTotalWt", ApplicantGPT.WTTotalWt);

		p.Add("WH1st", ApplicantGPT.WH1st);
		p.Add("WH2nd", ApplicantGPT.WH2nd);
		p.Add("WH3rd", ApplicantGPT.WH3rd);
		p.Add("WH4th", ApplicantGPT.WH4th);
		p.Add("WHTotalMk", ApplicantGPT.WHTotalMk);
		p.Add("WHTotalWt", ApplicantGPT.WHTotalWt);

		p.Add("STX", ApplicantGPT.STX);
		p.Add("STX2", ApplicantGPT.STX2);
		p.Add("STX3", ApplicantGPT.STX3);
		p.Add("STX4", ApplicantGPT.STX4);
		p.Add("STX5", ApplicantGPT.STX5);
		p.Add("STX6", ApplicantGPT.STX6);
		p.Add("STX7", ApplicantGPT.STX7);
		p.Add("STX8", ApplicantGPT.STX8);
		p.Add("STX9", ApplicantGPT.STX9);
		p.Add("STXTotalMk", ApplicantGPT.STXTotalMk);
		p.Add("STXTotalWt", ApplicantGPT.STXTotalWt);

		p.Add("PracParts", ApplicantGPT.PracParts);
		p.Add("PracETS", ApplicantGPT.PracETS);
		p.Add("PracCC", ApplicantGPT.PracCC);
		p.Add("PracSalutingTest", ApplicantGPT.PracSalutingTest);
		p.Add("PracTotalMk", ApplicantGPT.PracTotalMk);
		p.Add("PracTotalWt", ApplicantGPT.PracTotalWt);


		p.Add("FEWritten", ApplicantGPT.FEWritten);
		p.Add("FEPrac", ApplicantGPT.FEPrac);
		p.Add("FETotalMk", ApplicantGPT.FETotalMk );
		p.Add("FETotalWt", ApplicantGPT.FETotalWt);	
		
		p.Add("CEETotalMk", ApplicantGPT.CEETotalMk);
		p.Add("CEETotalWt", ApplicantGPT.CEETotalWt);
		
		
		p.Add("AdminGenAware", ApplicantGPT.AdminGenAware);
		p.Add("AdminDecipline", ApplicantGPT.AdminDecipline);
		p.Add("AdminTotalMk", ApplicantGPT.AdminTotalMk);
		p.Add("AdminTotalWt", ApplicantGPT.AdminTotalWt);
		
		p.Add("GrandTotalMk", ApplicantGPT.GrandTotalMk);
		p.Add("GrandTotalWt", ApplicantGPT.GrandTotalWt);
		p.Add("Remarks", ApplicantGPT.Remarks);
		

		p.Add("CreatedBy", ApplicantGPT.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantGPT_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateApplicantGPT(ApplicantGPTModel ApplicantGPT, LogModel logModel)
	{
		ClearCache(ApplicantGPTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantGPT.ApplicantGPTId);
		p.Add("ApplicantID", ApplicantGPT.ApplicantID);
		//p.Add("PostingDate", ApplicantGPT.PostingDate);
		p.Add("WT1st", ApplicantGPT.WT1st);
		p.Add("WT2nd", ApplicantGPT.WT2nd);
		p.Add("WT3rd", ApplicantGPT.WT3rd);
		p.Add("WT4th", ApplicantGPT.WT4th);
		p.Add("WT5th", ApplicantGPT.WT5th);
		p.Add("WT6th", ApplicantGPT.WT6th);
		p.Add("WTTotalMk", ApplicantGPT.WTTotalMk);
		p.Add("WTTotalWt", ApplicantGPT.WTTotalWt);

		p.Add("WH1st", ApplicantGPT.WH1st);
		p.Add("WH2nd", ApplicantGPT.WH2nd);
		p.Add("WH3rd", ApplicantGPT.WH3rd);
		p.Add("WH4th", ApplicantGPT.WH4th);
		p.Add("WHTotalMk", ApplicantGPT.WHTotalMk);
		p.Add("WHTotalWt", ApplicantGPT.WHTotalWt);

		p.Add("STX", ApplicantGPT.STX);
		p.Add("STX2", ApplicantGPT.STX2);
		p.Add("STX3", ApplicantGPT.STX3);
		p.Add("STX4", ApplicantGPT.STX4);
		p.Add("STX5", ApplicantGPT.STX5);
		p.Add("STX6", ApplicantGPT.STX6);
		p.Add("STX7", ApplicantGPT.STX7);
		p.Add("STX8", ApplicantGPT.STX8);
		p.Add("STX9", ApplicantGPT.STX9);
		p.Add("STXTotalMk", ApplicantGPT.STXTotalMk);
		p.Add("STXTotalWt", ApplicantGPT.STXTotalWt);

		p.Add("PracParts", ApplicantGPT.PracParts);
		p.Add("PracETS", ApplicantGPT.PracETS);
		p.Add("PracCC", ApplicantGPT.PracCC);
		p.Add("PracSalutingTest", ApplicantGPT.PracSalutingTest);
		p.Add("PracTotalMk", ApplicantGPT.PracTotalMk);
		p.Add("PracTotalWt", ApplicantGPT.PracTotalWt);


		p.Add("FEWritten", ApplicantGPT.FEWritten);
		p.Add("FEPrac", ApplicantGPT.FEPrac);
		p.Add("FETotalMk", ApplicantGPT.FETotalMk);
		p.Add("FETotalWt", ApplicantGPT.FETotalWt);


		p.Add("CEETotalMk", ApplicantGPT.CEETotalMk);
		p.Add("CEETotalWt", ApplicantGPT.CEETotalWt);

		p.Add("AdminGenAware", ApplicantGPT.AdminGenAware);
		p.Add("AdminDecipline", ApplicantGPT.AdminDecipline);
		p.Add("AdminTotalMk", ApplicantGPT.AdminTotalMk);
		p.Add("AdminTotalWt", ApplicantGPT.AdminTotalWt);

		p.Add("GrandTotalMk", ApplicantGPT.GrandTotalMk);
		p.Add("GrandTotalWt", ApplicantGPT.GrandTotalWt);
		p.Add("Remarks", ApplicantGPT.Remarks);

		p.Add("LastModifiedBy", ApplicantGPT.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantGPT_Update", p);
	}


	public async Task DeleteApplicantGPT(int ApplicantGPTId, LogModel logModel)
	{
		ClearCache(ApplicantGPTCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", ApplicantGPTId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_ApplicantGPT_Delete", p);
	}

	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case ApplicantGPTCache:
				var keys = _cache.Get<List<string>>(ApplicantGPTCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(ApplicantGPTCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}