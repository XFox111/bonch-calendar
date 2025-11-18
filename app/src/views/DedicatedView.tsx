import { Body1, Body2, makeStyles, tokens } from "@fluentui/react-components";
import type { ReactElement } from "react";
import strings from "../utils/strings";

export default function DedicatedView(): ReactElement
{
	const cls = useStyles();

	return (
		<section className={ cls.root }>
			<Body2 as="h2" align="center">{ strings.dedicated_h2 }</Body2>
			<Body1 as="p" align="center">{ strings.dedicated_p }</Body1>
			<a href="https://www.sut.ru/bonchnews/science/07-11-2022-pobedy-studentov-i-aspirantov-spbgut-na-radiofeste-2022" target="_blank" rel="noreferrer">
				<img src="/tios.jpg" className={ cls.image } />
			</a>
		</section>
	);
}

const useStyles = makeStyles({
	root:
	{
		display: "flex",
		boxSizing: "border-box",
		flexFlow: "column",
		alignItems: "center",
		gap: tokens.spacingVerticalL,
		padding: `200px ${tokens.spacingHorizontalM}`
	},
	image:
	{
		width: "100%",
		maxWidth: "600px",
		borderRadius: tokens.borderRadiusMedium,
		boxShadow: tokens.shadow16
	}
});
