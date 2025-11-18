import { LargeTitle, Subtitle1, Label, Dropdown, Button, Subtitle2, Body1, Option } from "@fluentui/react-components";
import { mergeClasses, useArrowNavigationGroup } from "@fluentui/react-components";
import type { SelectionEvents, OptionOnSelectData } from "@fluentui/react-components";
import { Copy24Regular, ArrowDownload24Regular, Checkmark24Regular } from "@fluentui/react-icons";
import { Slide, Stagger } from "@fluentui/react-motion-components-preview";
import { use, useCallback, useMemo, useState, type ReactElement } from "react";
import useTimeout from "../hooks/useTimeout";
import useStyles_MainView from "./MainView.styles";
import { fetchFaculties, fetchGroups } from "../utils/api";
import strings from "../utils/strings";

const facultiesPromise = fetchFaculties();

const getEntryOrEmpty = (entries: [string, string][], key: string): string =>
	entries.find(i => i[0] === key)?.[1] ?? "";

export default function MainView(): ReactElement
{
	const faculties: [string, string][] = use(facultiesPromise);
	const [facultyId, setFacultyId] = useState<string>("");

	const courses: number[] = useMemo(() => facultyId == "56682" ? [1, 2] : [1, 2, 3, 4, 5], [facultyId]);
	const [course, setCourse] = useState<number>(0);

	const [groups, setGroups] = useState<[string, string][] | null>(null);
	const [groupId, setGroupId] = useState<string>("");

	const icalUrl = useMemo(() => `${import.meta.env.VITE_BACKEND_HOST}/timetable/${facultyId}/${groupId}`, [groupId, facultyId]);

	const [showCta, setShowCta] = useState<boolean>(false);
	const [copyActive, triggerCopy] = useTimeout(3000);

	const navAttributes = useArrowNavigationGroup({ axis: "horizontal" });
	const cls = useStyles_MainView();

	const copyLink = useCallback((): void =>
	{
		navigator.clipboard.writeText(icalUrl);
		triggerCopy();
		setShowCta(true);
	}, [icalUrl, triggerCopy]);

	const onFacultySelect = useCallback((_: SelectionEvents, data: OptionOnSelectData): void =>
	{
		if (data.optionValue === facultyId)
			return;

		setFacultyId(data.optionValue!);
		setCourse(0);
		setGroupId("");
		setGroups(null);
	}, [facultyId]);

	const onCourseSelect = useCallback((courseNumber: number): void =>
	{
		if (courseNumber === course)
			return;

		setCourse(courseNumber);
		setGroupId("");
		setGroups(null);
		fetchGroups(facultyId, courseNumber).then(setGroups);
	}, [course, facultyId]);

	return (
		<section className={ cls.root }>
			<header className={ cls.stack }>
				<LargeTitle as="h1">
					{ strings.formatString(strings.title_p1, <span className={ cls.highlight }>{ strings.title_p2 }</span>) }
				</LargeTitle>
				<Subtitle1 as="p">
					{ strings.formatString(strings.subtitle_p1, <span className={ cls.highlight }>{ strings.subtitle_p2 }</span>) }
				</Subtitle1>
			</header>
			<div className={ mergeClasses(cls.stack, cls.form) }>
				<Slide visible appear>
					<div className={ cls.stack }>
						<Label htmlFor="faculty">{ strings.pickFaculty }</Label>
						<Dropdown id="faculty"
							value={ getEntryOrEmpty(faculties, facultyId) }
							onOptionSelect={ onFacultySelect }
							className={ cls.field }
							positioning={ { pinned: true, position: "below" } }
							button={
								<span className={ cls.truncatedText }>{ getEntryOrEmpty(faculties, facultyId) }</span>
							}>

							{ faculties.map(([id, name]) =>
								<Option key={ id } value={ id }>{ name }</Option>
							) }
						</Dropdown>
					</div>
				</Slide>
				<Slide visible={ facultyId !== "" }>
					<div className={ mergeClasses(cls.stack, facultyId === "" && cls.hidden) }>
						<Label>{ strings.pickCourse }</Label>
						<div { ...navAttributes }>
							{ courses.map(i =>
								<Button key={ i }
									className={ cls.courseButton }
									appearance={ course === i ? "primary" : "secondary" }
									onClick={ () => onCourseSelect(i) }>

									{ i }
								</Button>
							) }
						</div>
					</div>
				</Slide>
				<Slide visible={ course !== 0 && groups !== null }>
					<div className={ mergeClasses(cls.stack, course === 0 && cls.hidden) }>
						<Label as="label" htmlFor="group">{ strings.pickGroup }</Label>
						<Dropdown id="group"
							className={ cls.field }
							positioning={ { pinned: true, position: "below" } }
							value={ getEntryOrEmpty(groups ?? [], groupId) }
							onOptionSelect={ (_, e) => setGroupId(e.optionValue!) }>

							{ groups?.map(([id, name]) =>
								<Option key={ id } value={ id }>{ name }</Option>
							) }
							{ (groups?.length ?? 0) < 1 &&
								<Option disabled>{ strings.pickGroup_empty }</Option>
							}
						</Dropdown>
					</div>
				</Slide>
			</div>
			<div className={ cls.stack }>
				<Stagger visible={ groupId !== "" }>
					<Slide>
						<div className={ mergeClasses(cls.stack, groupId === "" && cls.hidden) }>
							<Subtitle2>{ strings.subscribe }</Subtitle2>
							<Button
								onClick={ copyLink }
								className={ mergeClasses(cls.field, copyActive && cls.copiedStyle) }
								iconPosition="after"
								title={ strings.copy }
								icon={ copyActive
									? <Checkmark24Regular className={ cls.copyIcon } />
									: <Copy24Regular className={ cls.copyIcon } />
								}>

								<span className={ cls.truncatedText }>{ icalUrl }</span>
							</Button>
						</div>
					</Slide>
					<Slide>
						<div className={ mergeClasses(cls.stack, groupId === "" && cls.hidden) }>
							<Body1>{ strings.or }</Body1>
							<Button as="a"
								appearance="subtle" icon={ <ArrowDownload24Regular /> }
								onClick={ () => setShowCta(true) }
								href={ icalUrl }>

								{ strings.download }
							</Button>
						</div>
					</Slide>
				</Stagger>
			</div>
			<Slide visible={ showCta }>
				<Subtitle2 as="p">{ strings.cta }</Subtitle2>
			</Slide>
		</section>
	);
}
