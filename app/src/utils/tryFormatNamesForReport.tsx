import { type TimetableHealthResponseEntry, fetchFaculties, fetchGroups } from "./api";
import strings from "./strings";

export async function tryFormatNamesForReport(report?: TimetableHealthResponseEntry): Promise<TimetableHealthResponseEntry | undefined>
{
	if (report === undefined)
		return report;

	if (report.status === "healthy")
		return report;

	const isGroupsDown: boolean = report.data["/groups"] !== undefined;
	const isTimetableDown: boolean = report.data["/timetable"] !== undefined;

	if (!isGroupsDown && !isTimetableDown)
		return report;

	let faculties: Record<string, string> | undefined = undefined;

	try { faculties = await fetchFaculties(); }
	catch { /* empty */ }

	const facultiesFormatted: string[] = [];

	if (report.data["/groups"] !== undefined)
		for (const faculty of report.data["/groups"])
		{
			const [facultyId, course] = faculty.split("/");

			if (faculties?.[facultyId] === undefined)
				facultiesFormatted.push(strings.formatString(strings.report_issue_groups_item_alt, facultyId, course) as string);

			else
				facultiesFormatted.push(strings.formatString(strings.report_issue_groups_item, faculties[facultyId], facultyId, course) as string);
		}

	const groups: Record<string, Record<string, string>> = {};
	const groupsFormatted: string[] = [];

	if (report.data["/timetable"] !== undefined)
		for (const group of report.data["/timetable"])
		{
			const [facultyId, groupId] = group.split("/");

			if (groups[facultyId] === undefined)
				try { groups[facultyId] = await fetchGroups(facultyId, 0); }
				catch { /* empty */ }

			if (groups[facultyId]?.[groupId] !== undefined && faculties?.[facultyId] !== undefined)
				groupsFormatted.push(`${groups[facultyId][groupId]} (${groupId}), ${faculties[facultyId]} (${facultyId})`);
			else if (faculties?.[facultyId] !== undefined)
				groupsFormatted.push(strings.formatString(strings.report_issue_timetable_item_alt1, groupId, faculties[facultyId], facultyId) as string)
			else if (groups[facultyId]?.[groupId] !== undefined)
				groupsFormatted.push(strings.formatString(strings.report_issue_timetable_item_alt2, groups[facultyId][groupId], groupId, facultyId) as string)
			else
				groupsFormatted.push(strings.formatString(strings.report_issue_timetable_item_alt3, groupId, facultyId) as string)
		}

	return {
		...report,
		data: {
			...report.data,
			["/groups"]: facultiesFormatted.length > 0 ? facultiesFormatted : undefined,
			["/timetable"]: groupsFormatted.length > 0 ? groupsFormatted : undefined
		}
	};
}
