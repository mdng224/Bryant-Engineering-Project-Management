namespace App.Infrastructure.Background;

public sealed record SendInstruction(string To, string Subject, string HtmlBody);