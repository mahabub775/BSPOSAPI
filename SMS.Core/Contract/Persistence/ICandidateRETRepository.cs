using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateRETRepository
{
	Task<List<CandidateRETModel>> GetCandidateRETsByCandidateId(int CandidateId);
	Task<CandidateRETModel> GetCandidateRETById(int CandidateRETId);
	Task<int> InsertCandidateRET(CandidateRETModel CandidateRET, LogModel logModel);
	Task UpdateCandidateRET(CandidateRETModel CandidateRET, LogModel logModel);
	Task DeleteCandidateRET(int CandidateRETId, LogModel logModel);
}