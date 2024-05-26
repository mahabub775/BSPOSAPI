using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICourseRepository
{
	Task<PaginatedListModel<CourseModel>> GetCourses(int pageNumber);
	Task<List<CourseModel>> GetDistinctCourses();

	
	Task<List<CourseModel>> GetTopMilitaryCourseTaken();
	Task<CourseModel> GetCourseById(int CourseId);
	Task<CourseModel> GetCourseByName(string CourseName);
	Task<int> InsertCourse(CourseModel Course, LogModel logModel);
	Task UpdateCourse(CourseModel Course, LogModel logModel);
	Task DeleteCourse(int CourseId, LogModel logModel);
	Task<List<CourseModel>> Export();
}