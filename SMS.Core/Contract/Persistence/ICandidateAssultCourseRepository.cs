using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateAssultCourseRepository
{
	Task<List<CandidateAssultCourseModel>> GetCandidateAssultCoursesByCandidateId(int CandidateId);
	Task<CandidateAssultCourseModel> GetCandidateAssultCourseById(int CandidateAssultCourseId);
	Task<int> InsertCandidateAssultCourse(CandidateAssultCourseModel CandidateAssultCourse, LogModel logModel);
	Task UpdateCandidateAssultCourse(CandidateAssultCourseModel CandidateAssultCourse, LogModel logModel);
	Task DeleteCandidateAssultCourse(int CandidateAssultCourseId, LogModel logModel);
}