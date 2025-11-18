import { makeStyles, tokens } from "@fluentui/react-components";

const useStyles_FaqView = makeStyles({
	root:
	{
		display: "flex",
		flexFlow: "column",
		justifyContent: "center",
		gap: tokens.spacingVerticalXXXL,
		width: "100%",
		maxWidth: "1200px",
		padding: `${tokens.spacingVerticalS} ${tokens.spacingVerticalM}`,
		userSelect: "text",
		boxSizing: "border-box",
		marginBottom: tokens.spacingVerticalXXXL
	},
	question:
	{
		display: "flex",
		flexFlow: "column",
		gap: tokens.spacingVerticalM
	},
	answer:
	{
		display: "flex",
		flexFlow: "column",
		gap: tokens.spacingVerticalSNudge,
		padding: `${tokens.spacingVerticalNone} ${tokens.spacingHorizontalM}`
	}
});

export default useStyles_FaqView;
