import { Body1, makeStyles } from "@fluentui/react-components";
import type { ReactElement } from "react";
import extLink from "../utils/extLink";
import strings from "../utils/strings";

const MY_WEBSITE = "https://xfox111.net";

export default function FooterView(): ReactElement
{
	const cls = useStyles();

	return (
		<footer className={ cls.root }>
			<div className={ cls.imageContainer }>
				<Body1 as="p" className={ cls.caption }>
					{strings.formatString(strings.footer_p1, <br />, extLink(MY_WEBSITE, strings.footer_p2))}
				</Body1>
				<img src="/footer.svg" />
			</div>
		</footer>
	);
}

const useStyles = makeStyles({
	root:
	{
		width: "100%",
		display: "flex",
		justifyContent: "flex-end",
		alignItems: "end"
	},
	imageContainer:
	{
		position: "relative",
		width: "100%",
		maxWidth: "400px",
	},
	caption:
	{
		position: "absolute",
		top: "24px",
		left: "72px",
	}
});
