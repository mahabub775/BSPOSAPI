using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IDeshboardRepository
{
	Task<DeshboardModel> GetDeshboardData();
}