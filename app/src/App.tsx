import { FluentProvider, makeStyles, type Theme } from "@fluentui/react-components";
import { type ReactElement } from "react";
import { useTheme } from "./hooks/useTheme";
import MainView from "./views/MainView";
import FaqView from "./views/FaqView";
import DedicatedView from "./views/DedicatedView";
import FooterView from "./views/FooterView";

export default function App(): ReactElement
{
	const theme: Theme = useTheme();
	const cls = useStyles();

	return (
		<FluentProvider theme={ theme }>
			<main className={ cls.root }>
				<MainView />
				<FaqView />
				<DedicatedView />
				<FooterView />
			</main>
		</FluentProvider>
	);
}

const useStyles = makeStyles({
	root:
	{
		display: "flex",
		flexFlow: "column",
		alignItems: "center",
		minHeight: "100vh"
	}
})
