using AIKernel.Tools.CapabilityModules.ChatHistoryCapability;
using System.Text;

namespace AIKernel.Tools.ChatHistoryScraper.Export;

public static class MdExporter
{
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
