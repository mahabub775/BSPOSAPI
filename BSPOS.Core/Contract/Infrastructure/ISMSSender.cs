using BSPOS.Core.Model;

namespace BSPOS.Core.Contract.Infrastructure;

public interface ISMSSender
{
	Task SendSMS(SMSModel sms);
}