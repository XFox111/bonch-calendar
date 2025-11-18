import { Body1, Subtitle1, Title3 } from "@fluentui/react-components";
import type { ReactElement } from "react";
import useStyles_FaqView from "./FaqView.styles";
import extLink from "../utils/extLink";
import strings from "../utils/strings";

const GITHUB_REPO = "https://github.com/xfox111/bonch-calendar";
const GITHUB_ISSUES = "https://github.com/xfox111/bonch-calendar/issues";
const GOOGLE_HELP = "https://support.google.com/calendar/answer/37100";
const OUTLOOK_HELP = "https://support.microsoft.com/office/import-or-subscribe-to-a-calendar-in-outlook-com-or-outlook-on-the-web-cff1429c-5af6-41ec-a5b4-74f2c278e98c";
const APPLE_HELP = "https://support.apple.com/en-us/102301";
const EMAIL = "mailto:feedback@xfox111.net";
const GUT_SCHEDULE_REPO = "https://github.com/xfox111/GUTSchedule";

export default function FaqView(): ReactElement
{
	const cls = useStyles_FaqView();

	return (
		<section className={ cls.root }>
			<Title3 align="center" as="h2" id="faq">{ strings.faq_h2 }</Title3>

			<div id="faq1" className={ cls.question }>
				<Subtitle1 as="h3">{ strings.question1_h3 }</Subtitle1>
				<div className={ cls.answer }>
					<Body1 as="p">{ strings.answer1_p1 }</Body1>
					<ul>
						<li>{ extLink(GOOGLE_HELP, strings.answer1_li1) }</li>
						<li>{ extLink(OUTLOOK_HELP, strings.answer1_li2) }</li>
						<li>{ extLink(APPLE_HELP, strings.answer1_li3) }</li>
					</ul>
					<Body1 as="p">{ strings.answer1_p2 }</Body1>
				</div>
			</div>
			<div id="faq2" className={ cls.question }>
				<Subtitle1 as="h3">{ strings.question2_h3 }</Subtitle1>
				<div className={ cls.answer }>
					<Body1 as="p">{ strings.answer2_p1 }</Body1>
				</div>
			</div>
			<div id="faq3" className={ cls.question }>
				<Subtitle1 as="h3">{ strings.question3_h3 }</Subtitle1>
				<div className={ cls.answer }>
					<Body1 as="p">{ strings.answer3_p1 }</Body1>
					<Body1 as="p">{ strings.answer3_p2 }</Body1>
				</div>
			</div>
			<div id="faq4" className={ cls.question }>
				<Subtitle1 as="h3">{ strings.question4_h3 }</Subtitle1>
				<div className={ cls.answer }>
					<Body1 as="p">{ strings.answer4_p1 }</Body1>
					<Body1 as="p">{ strings.answer4_p2 }</Body1>
				</div>
			</div>
			<div id="faq5" className={ cls.question }>
				<Subtitle1 as="h3">{ strings.question5_h3 }</Subtitle1>
				<div className={ cls.answer }>
					<Body1 as="p">{ strings.answer5_p1 }</Body1>
					<Body1 as="p">{ strings.answer5_p2 }</Body1>
				</div>
			</div>
			<div id="faq6" className={ cls.question }>
				<Subtitle1 as="h3">{ strings.question6_h3 }</Subtitle1>
				<div className={ cls.answer }>
					<Body1 as="p">
						{ strings.formatString(
							strings.answer6_p1,
							extLink(GITHUB_ISSUES, strings.answer6_p1_link1),
							extLink(EMAIL, strings.answer6_p1_link2)
						) }
					</Body1>
					<Body1 as="p">
						{ strings.formatString(strings.answer6_p2, extLink(GITHUB_REPO, strings.answer6_p2_link)) }
					</Body1>
				</div>
			</div>
			<div id="faq7" className={ cls.question }>
				<Subtitle1 as="h3">{ strings.question7_h3 }</Subtitle1>
				<div className={ cls.answer }>
					<Body1 as="p">
						{ strings.formatString(strings.answer7_p1, extLink(GITHUB_REPO, strings.answer7_p1_link)) }
					</Body1>
					<Body1 as="p">{ strings.answer7_p2}</Body1>
				</div>
			</div>
			<div id="faq8" className={ cls.question }>
				<Subtitle1 as="h3">{ strings.question8_h3 }</Subtitle1>
				<div className={ cls.answer }>
					<Body1 as="p">{ strings.answer8_p1 }</Body1>
					<Body1 as="p">
						{ strings.formatString(strings.answer8_p2,  extLink(GUT_SCHEDULE_REPO, strings.answer8_p2_link)) }
					</Body1>
				</div>
			</div>
		</section>
	);
}
