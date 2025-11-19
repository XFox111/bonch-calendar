using System.Text.RegularExpressions;

namespace BonchCalendar.Utils;

public static partial class ParserUtils
{
	public static (TimeSpan startTime, TimeSpan endTime) GetTimesFromLabel(string label)
	{
		(string startTime, string endTime) = label switch
		{
			"1" => ("9:00", "10:35"),
			"2" => ("10:45", "12:20"),
			"3" => ("13:00", "14:35"),
			"4" => ("14:45", "16:20"),
			"5" => ("16:30", "18:05"),
			"6" => ("18:15", "19:50"),
			"7" => ("20:00", "21:35"),
			"Ф1" => ("9:00", "10:30"),
			"Ф2" => ("10:30", "12:00"),
			"Ф3" => ("12:00", "13:30"),
			"Ф4" => ("13:30", "15:00"),
			"Ф5" => ("15:00", "16:30"),
			"Ф6" => ("16:30", "18:00"),
			"Ф7" => ("18:00", "19:30"),
			_ => throw new NotImplementedException(),
		};

		return (TimeSpan.Parse(startTime), TimeSpan.Parse(endTime));
	}

	[GeneratedRegex(@"^(?<number>\S+)\s\((?<start>\d+:\d+)-(?<end>\d+:\d+)\)$")]
	public static partial Regex TimeLabelRegex();

	[GeneratedRegex(@"\d+")]
	public static partial Regex NumberRegex();

	[GeneratedRegex(@"^(?<room>\d+),\sБ22\/(?<wing>\d)$")]
	public static partial Regex AuditoriumRegex();

	[GeneratedRegex(@"^(?<room>\d+),\sпр\.Большевиков,22,к\.(?<wing>\d)$")]
	public static partial Regex AuditoriumAltRegex();

	[GeneratedRegex(@"^(?<number>\d+)\s\((?<start>\d+\.\d+)-(?<end>\d+\.\d+)\)$")]
	public static partial Regex ExamTimeRegex();

	[GeneratedRegex(@"^(?<start>\d+:\d+)-(?<end>\d+:\d+)$")]
	public static partial Regex ExamTimeAltRegex();
}
