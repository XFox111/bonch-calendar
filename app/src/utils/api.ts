export const fetchFaculties = async (): Promise<[string, string][]> =>
{
	const res = await fetch(import.meta.env.VITE_BACKEND_HOST + "/faculties");
	return Object.entries(await res.json());
};

export const fetchGroups = async (facultyId: string, course: number): Promise<[string, string][]> =>
{
	const res = await fetch(`${import.meta.env.VITE_BACKEND_HOST}/groups?facultyId=${facultyId}&course=${course}`);
	return Object.entries(await res.json());
};
