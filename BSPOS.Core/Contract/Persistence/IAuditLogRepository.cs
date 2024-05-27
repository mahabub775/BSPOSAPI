﻿using BSPOS.Core.Model;

namespace BSPOS.Core.Contract.Persistence;

public interface IAuditLogRepository
{
	Task<PaginatedListModel<LogModel>> GetAuditLogs(int pageNumber);
	Task<LogModel> GetAuditLogById(int auditLogId);
	Task<int> InsertAuditLog(LogModel logModel);
	Task DeleteAuditLog(int auditLogId);
	Task<List<LogModel>> Export();
}