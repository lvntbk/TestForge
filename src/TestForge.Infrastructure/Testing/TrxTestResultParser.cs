using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using TestForge.Application.Testing;

namespace TestForge.Infrastructure.Testing;

public sealed class TrxTestResultParser : ITestResultParser
{
    public TestResultCounts Parse(string resultFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resultFilePath);

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(
            resultFilePath,
            settings);

        var document = XDocument.Load(reader);

        var counters = document
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName == "Counters")
            ?? throw new InvalidDataException(
                "TRX dosyasında Counters elementi bulunamadı.");

        return new TestResultCounts(
            ReadCounter(counters, "passed"),
            ReadCounter(counters, "failed"),
            ReadCounter(counters, "notExecuted"));
    }

    private static int ReadCounter(
        XElement counters,
        string attributeName)
    {
        var value = counters
            .Attribute(attributeName)?
            .Value;

        if (!int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var result) ||
            result < 0)
        {
            throw new InvalidDataException(
                $"TRX sayacı geçersiz: {attributeName}");
        }

        return result;
    }
}
