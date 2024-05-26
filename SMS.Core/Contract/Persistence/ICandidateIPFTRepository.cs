using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateIPFTRepository
{
	Task<List<CandidateIPFTModel>> GetCandidateIPFTsByCandidateId(int CandidateId);
	Task<CandidateIPFTModel> GetCandidateIPFTById(int CandidateIPFTId);
	Task<int> InsertCandidateIPFT(CandidateIPFTModel CandidateIPFT, LogModel logModel);
	Task UpdateCandidateIPFT(CandidateIPFTModel CandidateIPFT, LogModel logModel);
	Task DeleteCandidateIPFT(int CandidateIPFTId, LogModel logModel);
}