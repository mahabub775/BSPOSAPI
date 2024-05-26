using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateGPTRepository
{
	Task<List<CandidateGPTModel>> GetCandidateGPTsByCandidateId(int CandidateId);
	Task<CandidateGPTModel> GetCandidateGPTById(int CandidateGPTId);

	Task<List<CandidateGPTModel>> GetTopPerformers();
	Task<int> InsertCandidateGPT(CandidateGPTModel CandidateGPT, LogModel logModel);
	Task UpdateCandidateGPT(CandidateGPTModel CandidateGPT, LogModel logModel);
	Task DeleteCandidateGPT(int CandidateGPTId, LogModel logModel);

}