using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantAssultCourseRepository
{
	Task<List<ApplicantAssultCourseModel>> GetApplicantAssultCoursesByApplicantId(int ApplicantId);
	Task<ApplicantAssultCourseModel> GetApplicantAssultCourseById(int ApplicantAssultCourseId);
	Task<int> InsertApplicantAssultCourse(ApplicantAssultCourseModel ApplicantAssultCourse, LogModel logModel);
	Task UpdateApplicantAssultCourse(ApplicantAssultCourseModel ApplicantAssultCourse, LogModel logModel);
	Task DeleteApplicantAssultCourse(int ApplicantAssultCourseId, LogModel logModel);
}