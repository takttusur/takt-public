FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Takt.Services.WebApi/Takt.Services.WebApi.csproj", "src/Takt.Services.WebApi/"]
COPY ["src/Takt.Services.BusinessLogic/Takt.Services.BusinessLogic.csproj", "src/Takt.Services.BusinessLogic/"]
COPY ["src/Takt.Services.Infrastructure/Takt.Services.Infrastructure.csproj", "src/Takt.Services.Infrastructure/"]
RUN dotnet restore "src/Takt.Services.WebApi/Takt.Services.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Takt.Services.WebApi"
RUN dotnet build "./Takt.Services.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Takt.Services.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Takt.Services.WebApi.dll"]
