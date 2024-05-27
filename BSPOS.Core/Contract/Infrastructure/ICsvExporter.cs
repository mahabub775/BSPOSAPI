namespace BSPOS.Core.Contract.Infrastructure;

public interface ICsvExporter
{
	byte[] ExportToCsv<T>(List<T> items);
}