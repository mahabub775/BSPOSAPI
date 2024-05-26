using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateAddlQualificationRepository
{
	Task<List<CandidateAddlQualificationModel>> GetCandidateAddlQualificationsByCandidateId(int CandidateId);
	Task<CandidateAddlQualificationModel> GetCandidateAddlQualificationById(int CandidateAddlQualificationId);
	Task<int> InsertCandidateAddlQualification(CandidateAddlQualificationModel CandidateAddlQualification, LogModel logModel);
	Task UpdateCandidateAddlQualification(CandidateAddlQualificationModel CandidateAddlQualification, LogModel logModel);
	Task DeleteCandidateAddlQualification(int CandidateAddlQualificationId, LogModel logModel);
}