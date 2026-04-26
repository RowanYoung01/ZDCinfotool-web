using System.Text.Json.Serialization;

namespace ZdcReference.Features.Docs.Models;

public readonly record struct Document(string Name, string Url);
