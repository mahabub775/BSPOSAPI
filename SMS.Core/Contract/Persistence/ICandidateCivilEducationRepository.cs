using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateCivilEducationRepository
{
	Task<List<CandidateCivilEducationModel>> GetCandidateCivilEducationsByCandidateId(int CandidateId);
	Task<CandidateCivilEducationModel> GetCandidateCivilEducationById(int CandidateCivilEducationId);
	Task<int> InsertCandidateCivilEducation(CandidateCivilEducationModel CandidateCivilEducation, LogModel logModel);
	Task UpdateCandidateCivilEducation(CandidateCivilEducationModel CandidateCivilEducation, LogModel logModel);
	Task DeleteCandidateCivilEducation(int CandidateCivilEducationId, LogModel logModel);
}