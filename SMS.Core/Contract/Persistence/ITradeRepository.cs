using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ITradeRepository
{
	Task<PaginatedListModel<TradeModel>> GetTrades(int pageNumber);
	Task<List<TradeModel>> GetDistinctTrades();
	Task<TradeModel> GetTradeById(int TradeId);
	Task<TradeModel> GetTradeByName(string TradeName);
	Task<int> InsertTrade(TradeModel Trade, LogModel logModel);
	Task UpdateTrade(TradeModel Trade, LogModel logModel);
	Task DeleteTrade(int TradeId, LogModel logModel);
	Task<List<TradeModel>> Export();
}