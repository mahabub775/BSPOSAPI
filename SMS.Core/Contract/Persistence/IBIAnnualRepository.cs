using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IBIAnnualRepository
{
	Task<PaginatedListModel<BIAnnualModel>> GetBIAnnuals(int pageNumber);
	Task<List<BIAnnualModel>> GetDistinctBIAnnuals();
	Task<BIAnnualModel> GetBIAnnualById(int BIAnnualId);
	Task<BIAnnualModel> GetBIAnnualByName(string BIAnnualName);
	Task<int> InsertBIAnnual(BIAnnualModel BIAnnual, LogModel logModel);
	Task UpdateBIAnnual(BIAnnualModel BIAnnual, LogModel logModel);
	Task DeleteBIAnnual(int BIAnnualId, LogModel logModel);
	Task<List<BIAnnualModel>> Export();
}