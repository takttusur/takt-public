FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/Takt.Backend.WebApi/Takt.Backend.WebApi.csproj", "backend/Takt.Backend.WebApi/"]
RUN dotnet restore "backend/Takt.Backend.WebApi/Takt.Backend.WebApi.csproj"
COPY . .
WORKDIR "/src/backend/Takt.Backend.WebApi"
RUN dotnet build "./Takt.Backend.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Takt.Backend.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Takt.Backend.WebApi.dll"]
