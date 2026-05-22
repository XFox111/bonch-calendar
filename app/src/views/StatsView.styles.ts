import { makeStyles, tokens } from "@fluentui/react-components";

export const useStyles = makeStyles({
	root:
	{
		display: "flex",
		flexFlow: "column",
		marginBottom: "80px"
	},
	container:
	{
		display: "flex",
		padding: `${tokens.spacingVerticalS} ${tokens.spacingHorizontalMNudge}`,
		gap: tokens.spacingHorizontalMNudge,
		boxShadow: tokens.shadow4,
		borderRadius: tokens.borderRadiusMedium
	},
	statsButton:
	{
		pointerEvents: "none"
	},
	statsButtonIcon:
	{
		color: tokens.colorBrandForeground1
	},
	statusIconHealthy:
	{
		color: tokens.colorStatusSuccessBorderActive,
	},
	statusIconUnhealthy:
	{
		color: tokens.colorStatusDangerBorderActive,
	},
	reportSubtitle:
	{
		display: "flex",
		alignItems: "center",
		gap: tokens.spacingHorizontalS
	},
	reportContent:
	{
		userSelect: "text"
	}
});
