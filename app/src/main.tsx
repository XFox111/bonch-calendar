import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import strings from "./utils/strings.ts";

const preferredLanguages = navigator.languages.map(lang => lang.split("-")[0].toLocaleLowerCase());

if (
	(preferredLanguages.includes("ru")&& !window.location.pathname.startsWith("/en")) ||
	window.location.pathname.startsWith("/ru")
)
	strings.setLanguage("ru");
else
{
	strings.setLanguage("en");
	document.title = strings.formatString(strings.title_p1, strings.title_p2) as string;
}

createRoot(document.getElementById("root")!).render(
	<StrictMode>
		<App />
	</StrictMode>
);
