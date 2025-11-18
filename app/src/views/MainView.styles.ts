import { makeStyles, tokens, shorthands } from "@fluentui/react-components";


const useStyles_MainView = makeStyles({
	root:
	{
		display: "flex",
		flexFlow: "column",
		gap: tokens.spacingVerticalXXXL,
		justifyContent: "center",
		minHeight: "90vh",
		alignItems: "center",
		padding: `${tokens.spacingVerticalL} ${tokens.spacingHorizontalL}`,

		"& p":
		{
			textAlign: "center"
		}
	},
	highlight:
	{
		color: tokens.colorBrandForeground1
	},
	courseButton:
	{
		minWidth: "48px",
		borderRadius: tokens.borderRadiusNone,
		borderRightWidth: 0,

		"&:first-of-type":
		{
			borderStartStartRadius: tokens.borderRadiusCircular,
			borderEndStartRadius: tokens.borderRadiusCircular
		},

		"&:last-of-type":
		{
			borderStartEndRadius: tokens.borderRadiusCircular,
			borderEndEndRadius: tokens.borderRadiusCircular,
			borderRightWidth: tokens.strokeWidthThin,
		},
	},
	stack:
	{
		display: "flex",
		flexFlow: "column",
		alignItems: "center",
		gap: tokens.spacingVerticalSNudge
	},
	form:
	{
		gap: tokens.spacingVerticalL
	},
	copiedStyle:
	{
		color: tokens.colorStatusSuccessForeground1 + " !important",
		...shorthands.borderColor(tokens.colorStatusSuccessBorder1 + " !important")
	},
	field:
	{
		width: "250px"
	},
	copyIcon:
	{
		animationName: "scaleUpFade",
		animationDuration: tokens.durationFast,
		animationTimingFunction: tokens.curveEasyEaseMax
	},
	hidden:
	{
		pointerEvents: "none"
	},
	truncatedText:
	{
		overflowX: "hidden",
		textOverflow: "ellipsis",
		whiteSpace: "nowrap"
	}
});

export default useStyles_MainView;
