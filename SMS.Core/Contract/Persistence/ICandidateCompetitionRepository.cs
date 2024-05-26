using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateCompetitionRepository
{
	Task<List<CandidateCompetitionModel>> GetCandidateCompetitionsByCandidateId(int CandidateId);
	Task<CandidateCompetitionModel> GetCandidateCompetitionById(int CandidateCompetitionId);
	Task<int> InsertCandidateCompetition(CandidateCompetitionModel CandidateCompetition, LogModel logModel);
	Task UpdateCandidateCompetition(CandidateCompetitionModel CandidateCompetition, LogModel logModel);
	Task DeleteCandidateCompetition(int CandidateCompetitionId, LogModel logModel);
}