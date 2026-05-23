import { Button, Dialog, DialogBody, DialogContent, DialogSurface, DialogTitle, DialogTrigger, Divider, Subtitle2 } from "@fluentui/react-components";
import { ArrowTrendingLinesFilled, CheckmarkCircleFilled, Dismiss24Regular, WarningFilled } from "@fluentui/react-icons";
import { use, useMemo, type ReactElement } from "react";
import { fetchHealth, fetchStats, type StatsResponse, type TimetableHealthResponseEntry } from "../utils/api";
import strings from "../utils/strings";
import { tryFormatNamesForReport } from "../utils/tryFormatNamesForReport";
import { useStyles } from "./StatsView.styles";

const healthPromise = fetchHealth().then(i => i.entries?.["timetable_website"]).then(tryFormatNamesForReport);
const statsPromise = fetchStats();

export default function StatsView(): ReactElement
{
	const cls = useStyles();

	const health: TimetableHealthResponseEntry | undefined = use(healthPromise);
	const stats: StatsResponse = use(statsPromise);

	const issueCounter: number = useMemo(() =>
	{
		let counter: number = 0;

		if (health === undefined)
			return 1;

		if (health.data["/faculties"] !== undefined)
			counter++;

		counter += health.data["/groups"]?.length ?? 0;
		counter += health.data["/timetable"]?.length ?? 0;

		return counter;
	}, [health]);

	return (
		<div className={ cls.root }>
			<div className={ cls.container }>
				{ stats.activeUsers > 3 &&
					<>
						<Button
							className={ cls.statsButton } tabIndex={ -1 }
							icon={ <ArrowTrendingLinesFilled className={ cls.statsButtonIcon } /> }
							appearance="subtle"
						>
							{ strings.formatString(strings.users, stats.activeUsers) }
						</Button>
						<Divider vertical />
					</>
				}
				<Dialog>
					<DialogTrigger>
						{ health?.status === "healthy" ?
							<Button icon={ <CheckmarkCircleFilled className={ cls.statusIconHealthy } /> } appearance="subtle">
								{ strings.status_ok }
							</Button>
							:
							<Button icon={ <WarningFilled className={ cls.statusIconUnhealthy } /> } appearance="subtle">
								{ strings.status_unhealthy }
							</Button>
						}
					</DialogTrigger>

					<DialogSurface>
						<DialogBody>
							<DialogTitle
								action={
									<DialogTrigger action="close">
										<Button
											appearance="subtle"
											aria-label={ strings.report_close }
											icon={ <Dismiss24Regular /> }
										/>
									</DialogTrigger>
								}
							>
								{ strings.report_title }
							</DialogTitle>
							<DialogContent className={ cls.reportContent }>
								{ health?.status === "healthy" ?
									<div className={ cls.reportSubtitle }>
										<CheckmarkCircleFilled className={ cls.statusIconHealthy } fontSize={ 24 } />
										<Subtitle2>{ strings.report_subtitle_ok }</Subtitle2>
									</div>
									:
									<div className={ cls.reportSubtitle }>
										<WarningFilled className={ cls.statusIconUnhealthy } fontSize={ 24 } />
										<Subtitle2>
											{ strings.formatString(strings.report_subtitle_unhealthy, issueCounter) }
										</Subtitle2>
									</div>
								}
								{ health?.status !== "healthy" &&
									<ul>
										{ health === undefined &&
											<li>{ strings.report_issue_backend }</li>
										}
										{ health?.data["/faculties"] !== undefined &&
											<li>{ strings.report_issue_faculties }</li>
										}
										{ health?.data["/groups"] !== undefined &&
											<li>
												{ strings.report_issue_groups }
												<ul>
													{ health.data["/groups"].map((i, index) =>
														<li key={ index }>{ i }</li>
													) }
												</ul>
											</li>
										}
										{ health?.data["/timetable"] !== undefined &&
											<li>
												{ strings.report_issue_timetable }
												<ul>
													{ health.data["/timetable"].map((i, index) =>
														<li key={ index }>{ i }</li>
													) }
												</ul>
											</li>
										}
									</ul>
								}
							</DialogContent>
						</DialogBody>
					</DialogSurface>
				</Dialog>
			</div>
		</div>
	);
}
