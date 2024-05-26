using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantRETRepository
{
	Task<List<ApplicantRETModel>> GetApplicantRETsByApplicantId(int ApplicantId);
	Task<ApplicantRETModel> GetApplicantRETById(int ApplicantRETId);
	Task<int> InsertApplicantRET(ApplicantRETModel ApplicantRET, LogModel logModel);
	Task UpdateApplicantRET(ApplicantRETModel ApplicantRET, LogModel logModel);
	Task DeleteApplicantRET(int ApplicantRETId, LogModel logModel);
}