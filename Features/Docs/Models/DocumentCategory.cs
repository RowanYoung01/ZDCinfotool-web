using System.Text.Json.Serialization;

namespace ZdcReference.Features.Docs.Models;

public record DocumentCategory
{
    public string Name { get; init; } = "";
    public List<Document> Documents { get; init; } = [];
}
