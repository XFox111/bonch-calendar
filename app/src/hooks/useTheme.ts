import { webDarkTheme, webLightTheme, type Theme } from "@fluentui/react-components";
import { useEffect, useState } from "react";

const media: MediaQueryList = window.matchMedia("(prefers-color-scheme: dark)");
const getTheme = (isDark: boolean) => isDark ? darkTheme : lightTheme;

const baseTheme: Partial<Theme> =
{
	fontFamilyBase: "\"Fira Sans\", sans-serif",
	colorBrandForeground1: "#f68b1f",
	colorBrandStroke1: "#f68b1f",
	colorBrandForegroundLink: "#f68b1f",
	colorBrandForegroundLinkHover: "#c36e18",
	colorBrandForegroundLinkPressed: "#a95f15",
	colorBrandStroke2Contrast: "#FDE6CE",
	colorBrandBackground: "#f68b1f",
	colorBrandBackgroundHover: "#c36e18",
	colorNeutralForeground2BrandHover: "#c36e18",
	colorBrandBackgroundPressed: "#a95f15",
	colorCompoundBrandStroke: "#f68b1f",
	colorCompoundBrandStrokePressed: "#a95f15"
};

const lightTheme: Theme =
{
	...webLightTheme, ...baseTheme,
	colorNeutralForeground1: "#000000",
	colorNeutralForeground2: "#4D4D4D"
};

const darkTheme: Theme =
{
	...webDarkTheme, ...baseTheme,
	colorNeutralBackground2: "#4D4D4D"
};

export function useTheme(): Theme
{
	const [theme, setTheme] = useState<Theme>(getTheme(media.matches));

	useEffect(() =>
	{
		const updateTheme = (args: MediaQueryListEvent) => setTheme(getTheme(args.matches));
		media.addEventListener("change", updateTheme);
		return () => media.removeEventListener("change", updateTheme);
	}, []);

	return theme;
}
