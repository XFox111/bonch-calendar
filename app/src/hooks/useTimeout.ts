import { useCallback, useState } from "react";

export default function useTimeout(timeout: number): [boolean, () => void]
{
	const [isActive, setActive] = useState<boolean>(false);

	const trigger = useCallback(() =>
	{
		if (isActive)
			return;

		setActive(true);
		setTimeout(() => setActive(false), timeout);
	}, [timeout, isActive]);

	return [isActive, trigger];
}
