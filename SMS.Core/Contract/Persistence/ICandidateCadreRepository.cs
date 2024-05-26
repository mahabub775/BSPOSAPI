using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateCadreRepository
{
	Task<List<CandidateCadreModel>> GetCandidateCadresByCandidateId(int CandidateId);
	Task<CandidateCadreModel> GetCandidateCadreById(int CandidateCadreId);
	Task<int> InsertCandidateCadre(CandidateCadreModel CandidateCadre, LogModel logModel);
	Task UpdateCandidateCadre(CandidateCadreModel CandidateCadre, LogModel logModel);
	Task DeleteCandidateCadre(int CandidateCadreId, LogModel logModel);
}