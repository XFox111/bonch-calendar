# Contributing Guidelines

This document will guide you through the process of contributing to this project. We welcome contributions from the community and appreciate your efforts to improve it.

> [!NOTE]
> This document is a work in progress. More information will be added later.

## Asking questions and providing feedback

If you have questions about the project or need help with something, please don't hesitate to [open an issue](https://github.com/XFox111/bonch-calendar/issues/new), or send me an email to [feedback@xfox111.net](mailto:feedback@xfox111.net).

## Before you start

Before you start working on the codebase, please make sure that there is a related issue on GitHub and that you have been assigned to it.

To do so, find an existing issue that you would like to work on, or create a new one if it doesn't exist. Once you have an issue, please comment on it to let us know that you would like to work on it.

This will help to avoid duplicate work.

### What should I know/learn before I start?

Depending on the area of the codebase you want to work on, it's good to have some basic knowledge of:
- Git and GitHub
- .NET and C#
- React and TypeScript
- Docker

### AI Usage Policy

We have nothing against the use of AI tools to assist in development, but to maintain the quality of the codebase and avoid wasting time on low-quality PRs, we ask you to follow these guidelines when using AI tools:

1. **Understand the code you submit**: you should be able to understand and expain any line of code you submit in your own words.
1. **Make sure your code actually works**: before submitting a PR, make sure to test your code and verify that it works as expected. This includes manual testing and/or writing automated tests.
1. **Write your own words**: do not copy/paste AI-generated PR descriptions, comments, etc. on GitHub. Make sure it's concise and to the point.
1. **Disclose AI usage**: Note in the PR description if and how you have used AI tools to assist in the creation of the PR.

> [!IMPORTANT]
> If the PR appears to be low-effort AI slop, it may be close without review.

#### Disclosure examples

```
Used an agentic AI to generate the implementation of the new API endpoint.
I have manually reviewed and tested it before submitting.
```

```
- [X] I have used Copilot (or other AI tools) for autocomplete. I have not used agentic AI tools. I have manually reviewed this PR.
```

```
- [X] I have not used AI tools to assist in the creation of this pull request.
```

> [!IMPORTANT]
> Even if you have not used AI tools, you still need to note that in the PR descrption.
>
> PR description template already includes everything you need.

## Getting started

### Working on the codebase

#### 1. Setting up your development environment

Following tools are needed to set up your development environment:

- .NET SDK 10.0 (required for building backend)
- Node.js 26 with npm (required for building frontend)
- Docker (required for building comlete images)
- VS Code (highly recommended)

> [!TIP]
> You can use [Dev Containers](https://code.visualstudio.com/docs/remote/containers) extension in VS Code to set up a development environment with all the required tools pre-installed.

#### 2. Running the project

Following commands can be used to run the project:

```bash
# Run the API
cd api && dotnet run
# Run the frontend, requires the API to be running
cd app && npm run dev # -- --host if you use devcontainers
```

> [!TIP]
> If you use VS Code you can use `Ctrl+Shift+P > Tasks: Run Task` to run and build the project.

#### 3. Testing the API

You can go to `http://localhost:8080/scalar` to test the API.

### Submitting a pull request

Before submitting a pull request, please ensure that:

- Your commits and PR title follow [Conventional Commits specification](https://www.conventionalcommits.org/):

	`<type>[optional scope]: <description>`

	Examples:
	```
	fix: fix a bug in the shortener endpoint
	feat(auth): add support for OIDC authentication
	build(deps): update dependencies to latest versions
	```
- You PR has a detailed description and mentions the issue it resolves.
- You have run formatters:
	```bash
	# For API project
	dotnet format api/BonchCalendar.slnx
	# For frontend
	cd app && npm run format
	```
