import { Link } from "@fluentui/react-components";
import type { ReactElement } from "react";

const extLink = (url: string, text: string): ReactElement =>
	<Link href={ url } target="_blank" rel="noreferrer">{ text }</Link>;

export default extLink;
