using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantGPTRepository
{
	Task<List<ApplicantGPTModel>> GetApplicantGPTsByApplicantId(int ApplicantId);
	Task<ApplicantGPTModel> GetApplicantGPTById(int ApplicantGPTId);

	Task<List<ApplicantGPTModel>> GetTopPerformers();
	Task<int> InsertApplicantGPT(ApplicantGPTModel ApplicantGPT, LogModel logModel);
	Task UpdateApplicantGPT(ApplicantGPTModel ApplicantGPT, LogModel logModel);
	Task DeleteApplicantGPT(int ApplicantGPTId, LogModel logModel);

}