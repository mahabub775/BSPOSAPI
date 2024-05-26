using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IBrigadeRepository
{
	Task<PaginatedListModel<BrigadeModel>> GetBrigades(int pageNumber);
	Task<List<BrigadeModel>> GetDistinctBrigades();
	Task<BrigadeModel> GetBrigadeById(int BrigadeId);
	Task<BrigadeModel> GetBrigadeByName(string BrigadeName);
	Task<int> InsertBrigade(BrigadeModel Brigade, LogModel logModel);
	Task UpdateBrigade(BrigadeModel Brigade, LogModel logModel);
	Task DeleteBrigade(int BrigadeId, LogModel logModel);
	Task<List<BrigadeModel>> Export();
}