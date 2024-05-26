using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IBAASectionRepository
{
	Task<PaginatedListModel<BAASectionModel>> GetBAASections(int pageNumber);
	Task<List<BAASectionModel>> GetDistinctBAASections();
	Task<BAASectionModel> GetBAASectionById(int BAASectionId);
	Task<BAASectionModel> GetBAASectionByName(string BAASectionName);
	Task<int> InsertBAASection(BAASectionModel BAASection, LogModel logModel);
	Task UpdateBAASection(BAASectionModel BAASection, LogModel logModel);
	Task DeleteBAASection(int BAASectionId, LogModel logModel);
	Task<List<BAASectionModel>> Export();
}