FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /build

ADD *.csproj .
RUN dotnet restore

ADD . ./
RUN dotnet publish --no-restore --configuration Release --output /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS prod
WORKDIR /app

COPY --from=build /out/* .

ENTRYPOINT [ "dotnet", "BonchCalendar.dll" ]
