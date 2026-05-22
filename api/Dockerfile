FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /build

ADD *.csproj .
RUN dotnet restore

ADD . ./
RUN dotnet publish --no-restore --configuration Release --output /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS prod
WORKDIR /app

COPY --from=build /out/* .

EXPOSE 8080
HEALTHCHECK --interval=60s --retries=3 --start-period=5s --timeout=10s \
	CMD wget --no-verbose --tries 1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT [ "dotnet", "BonchCalendar.dll" ]
