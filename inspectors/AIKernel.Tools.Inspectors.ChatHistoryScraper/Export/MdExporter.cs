using AIKernel.Tools.Capability.RomStorage.ChatHistory;
using System.Text;

namespace AIKernel.Tools.Inspectors.ChatHistoryScraper.Export;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.MdExporter']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.MdExporter']" />
public static class MdExporter
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.MdExporter.ToMarkdown']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.MdExporter.ToMarkdown']" />
    public static string ToMarkdown(IReadOnlyList<ChatHistoryRecord> records)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Chat History");
        sb.AppendLine();

        int turn = 1;
        foreach (var r in records)
        {
            sb.AppendLine($"## Turn {turn++} ({r.Role})");
            sb.AppendLine(r.Timestamp.ToString("o"));
            sb.AppendLine();
            sb.AppendLine(r.Content.Trim());
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
