using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateCertificationRepository
{
	Task<List<CandidateCertificationModel>> GetCandidateCertificationsByCandidateId(int CandidateId);
	Task<CandidateCertificationModel> GetCandidateCertificationById(int CandidateCertificationId);
	Task<int> InsertCandidateCertification(CandidateCertificationModel CandidateCertification, LogModel logModel);
	Task UpdateCandidateCertification(CandidateCertificationModel CandidateCertification, LogModel logModel);
	Task DeleteCandidateCertification(int CandidateCertificationId, LogModel logModel);
}