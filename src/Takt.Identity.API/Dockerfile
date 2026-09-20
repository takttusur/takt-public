FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Takt.Identity.API/Takt.Identity.API.csproj", "src/Takt.Identity.API/"]
RUN dotnet restore "src/Takt.Identity.API/Takt.Identity.API.csproj"
COPY . .
WORKDIR "/src/src/Takt.Identity.API"
RUN dotnet build "./Takt.Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Takt.Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=5s --retries=5 CMD curl --fail http://127.0.0.1:8080/health/ready || exit 1
ENTRYPOINT ["dotnet", "Takt.Identity.API.dll"]
