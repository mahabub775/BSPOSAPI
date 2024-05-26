using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IQualificationRepository
{
	Task<PaginatedListModel<QualificationModel>> GetQualifications(int pageNumber);
	Task<List<QualificationModel>> GetDistinctQualifications();
	Task<QualificationModel> GetQualificationById(int QualificationId);
	Task<QualificationModel> GetQualificationByName(string QualificationName);
	Task<int> InsertQualification(QualificationModel Qualification, LogModel logModel);
	Task UpdateQualification(QualificationModel Qualification, LogModel logModel);
	Task DeleteQualification(int QualificationId, LogModel logModel);
	Task<List<QualificationModel>> Export();
}