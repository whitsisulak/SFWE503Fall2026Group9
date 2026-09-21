# Build context is the REPO ROOT (see docker-compose.yml), not this folder,
# because the API project references ../Clms.Shared.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore as its own layer so code edits don't invalidate the package cache.
COPY Directory.Build.props ./
COPY src/Clms.Shared/Clms.Shared.csproj src/Clms.Shared/
COPY src/Clms.Api/Clms.Api.csproj       src/Clms.Api/
RUN dotnet restore src/Clms.Api/Clms.Api.csproj

COPY src/Clms.Shared/ src/Clms.Shared/
COPY src/Clms.Api/    src/Clms.Api/
RUN dotnet publish src/Clms.Api/Clms.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Instrument drop folder is bind-mounted here by docker-compose.
RUN mkdir -p /data/instrument-drop

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Clms.Api.dll"]
