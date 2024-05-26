using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateQuizCompetitionRepository
{
	Task<List<CandidateQuizCompetitionModel>> GetCandidateQuizCompetitionsByCandidateId(int CandidateId);
	Task<CandidateQuizCompetitionModel> GetCandidateQuizCompetitionById(int CandidateQuizCompetitionId);
	Task<int> InsertCandidateQuizCompetition(CandidateQuizCompetitionModel CandidateQuizCompetition, LogModel logModel);
	Task UpdateCandidateQuizCompetition(CandidateQuizCompetitionModel CandidateQuizCompetition, LogModel logModel);
	Task DeleteCandidateQuizCompetition(int CandidateQuizCompetitionId, LogModel logModel);
}