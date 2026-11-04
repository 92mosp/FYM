FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY API/FYMTechnology.Api/FYMTechnology.Api.csproj API/FYMTechnology.Api/
RUN dotnet restore API/FYMTechnology.Api/FYMTechnology.Api.csproj
COPY . .
RUN dotnet publish API/FYMTechnology.Api/FYMTechnology.Api.csproj -c Release -o /app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "FYMTechnology.Api.dll"]
