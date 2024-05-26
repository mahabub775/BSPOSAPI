using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateDisciplineRepository
{
	Task<List<CandidateDisciplineModel>> GetCandidateDisciplinesByCandidateId(int CandidateId);
	Task<CandidateDisciplineModel> GetCandidateDisciplineById(int CandidateDisciplineId);
	Task<int> InsertCandidateDiscipline(CandidateDisciplineModel CandidateDiscipline, LogModel logModel);
	Task UpdateCandidateDiscipline(CandidateDisciplineModel CandidateDiscipline, LogModel logModel);
	Task DeleteCandidateDiscipline(int CandidateDisciplinId, LogModel logModel);
}