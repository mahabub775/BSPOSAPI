using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IRankRepository
{
	Task<PaginatedListModel<RankModel>> GetRanks(int pageNumber);
	Task<List<RankModel>> GetDistinctRanks();
	Task<RankModel> GetRankById(int RankId);
	Task<RankModel> GetRankByName(string RankName);
	Task<int> InsertRank(RankModel Rank, LogModel logModel);
	Task UpdateRank(RankModel Rank, LogModel logModel);
	Task DeleteRank(int RankId, LogModel logModel);
	Task<List<RankModel>> Export();
}