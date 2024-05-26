using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantCadreRepository
{
	Task<List<ApplicantCadreModel>> GetApplicantCadresByApplicantId(int ApplicantId);
	Task<ApplicantCadreModel> GetApplicantCadreById(int ApplicantCadreId);
	Task<int> InsertApplicantCadre(ApplicantCadreModel ApplicantCadre, LogModel logModel);
	Task UpdateApplicantCadre(ApplicantCadreModel ApplicantCadre, LogModel logModel);
	Task DeleteApplicantCadre(int ApplicantCadreId, LogModel logModel);
}