using System;
using System.Collections.Generic;
using System.Text;

namespace AIKernel.Tools.ChatHistoryScraper.Models;

public sealed record ChatMessage(
    string Role,
    string Text,
    string? Timestamp
);
