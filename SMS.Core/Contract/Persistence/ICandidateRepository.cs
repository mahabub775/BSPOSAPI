using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateRepository
{
	Task<PaginatedListModel<CandidateModel>> GetCandidates(int pageNumber, int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId , string ArmyNo, string Name );
	Task<List<CandidateModel>> GetGroupReport(int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId );
	Task<CandidateModel> GetCandidateById(int CandidateId);
	Task<CandidateModel> GetCandidateByName(string CandidateName);
	
	Task<int> InsertCandidate(CandidateModel Candidate, LogModel logModel);
	Task UpdateCandidate(CandidateModel Candidate, LogModel logModel);
	Task<List<CandidateModel>> Export();
}