const timeout: number = 5000;

export const fetchFaculties = (): Promise<Record<string, string>> =>
	fetchApi("/faculties", {});

export const fetchGroups = (facultyId: string, year: number): Promise<Record<string, string>> =>
	fetchApi(`/groups?facultyId=${facultyId}&year=${year}`, {});

export const fetchStats = async (): Promise<StatsResponse> =>
	fetchApi("/stats", {
		activeUsers: 0
	});

export const fetchHealth = async (): Promise<HealthResponse> =>
	fetchApi("/health", {} as HealthResponse, true);

async function fetchApi<T>(path: string, defaultValue: T, alwaysReturnResponse: boolean = false): Promise<T>
{
	try
	{
		const res = await fetch(new URL(path, import.meta.env.VITE_BACKEND_HOST), {
			signal: AbortSignal.timeout(timeout)
		});

		if (!res.ok && !alwaysReturnResponse)
			return defaultValue;

		return await res.json();
	}
	catch
	{
		return defaultValue;
	}
}

export type StatsResponse =
	{
		activeUsers: number;
	};

export type HealthResponse =
	{
		status: HealthStatus;
		totalDuration: string;
		entries: {
			["timetable_website"]: TimetableHealthResponseEntry;
		};
	};

export type HealthStatus = "healthy" | "unhealthy" | "degraded";

export type TimetableHealthResponseEntry =
	{
		status: HealthStatus;
		description?: string;
		duration: string;
		data:
		{
			"/faculties"?: false,
			"/groups"?: string[],
			"/timetable"?: string[];
		};
	};
