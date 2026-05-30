using AIKernel.Tools.CapabilityModules.ChatHistoryCapability;
using System.Text;

namespace ChatHistoryScraper.Export;

public static class RomExporter
{
    public static string ToRom(IReadOnlyList<ChatHistoryRecord> records)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# ROM:ChatHistory");
        sb.AppendLine("@type: conversation");
        sb.AppendLine($"@generated: {DateTimeOffset.UtcNow:o}");
        sb.AppendLine();

        int turn = 1;

        foreach (var r in records)
        {
            sb.AppendLine($"## Turn:{turn++}");
            sb.AppendLine($"@role: {r.Role}");
            sb.AppendLine($"@time: {r.Timestamp:o}");
            sb.AppendLine();
            sb.AppendLine(r.Content.Trim());
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
