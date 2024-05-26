using CsvHelper;
using SMS.Core.Contract.Infrastructure;
using System.Globalization;

namespace SMS.Infrastructure;

public class CsvExporter : ICsvExporter
{
	public byte[] ExportToCsv<T>(List<T> items)
	{
		using var memoryStream = new MemoryStream();
		using (var streamWriter = new StreamWriter(memoryStream))
		{
			using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
			csvWriter.WriteRecords(items);
		}

		return memoryStream.ToArray();
	}
}