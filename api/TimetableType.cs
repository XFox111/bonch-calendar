namespace BonchCalendar;

/// <summary>
/// Types of timetable documents retrieved from sut.ru API.
/// </summary>
public enum TimetableType
{
	/// <summary>
	/// Regular timetable document (Занятия).
	/// </summary>
	Classes = 1,

	/// <summary>
	/// Exams timetable document (Экзаменационная сессия).
	/// </summary>
	Exams = 2,

	/// <summary>
	/// Exams timetable for extramural students document (Сессия для заочников).
	/// </summary>
	ExamsForExtramural = 4,

	/// <summary>
	/// Attestations timetable document (Зачеты).
	/// </summary>
	Attestations = 14
}
