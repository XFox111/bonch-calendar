using System.Text.Json.Serialization;
using BonchCalendar.Health;

namespace BonchCalendar;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(StatsResponse))]
[JsonSerializable(typeof(Dictionary<int, string>))]
[JsonSerializable(typeof(HealthResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
