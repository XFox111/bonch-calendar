FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine-aot AS build
WORKDIR /build

ADD --link . .
RUN --mount=type=cache,target=/root/.nuget \
	--mount=type=cache,target=/source/bin \
	--mount=type=cache,target=/source/obj \
	dotnet publish --output /out BonchCalendar.csproj \
	&& rm /out/*.dbg /out/*.Development.json

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS prod
WORKDIR /app

COPY --link --from=build /out/* .
USER $APP_UID

HEALTHCHECK --interval=60s --retries=3 --start-period=5s --timeout=10s \
	CMD wget --no-verbose --tries 1 --spider http://localhost:8080/health || exit 1

EXPOSE 8080
ENTRYPOINT [ "./BonchCalendar" ]
