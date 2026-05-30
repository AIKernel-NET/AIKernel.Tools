using System;
using System.Collections.Generic;
using System.Text;

namespace AIKernel.Tools.ChatHistoryScraper.Models;

public sealed record ChatHistory(
    IReadOnlyList<ChatMessage> Messages
);