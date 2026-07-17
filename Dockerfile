FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CompeTournament.Shared/CompeTournament.Shared.csproj CompeTournament.Shared/
COPY CompeTournament.Backend/CompeTournament.Backend.csproj CompeTournament.Backend/
RUN dotnet restore CompeTournament.Backend/CompeTournament.Backend.csproj

COPY CompeTournament.Shared/ CompeTournament.Shared/
COPY CompeTournament.Backend/ CompeTournament.Backend/
RUN dotnet publish CompeTournament.Backend/CompeTournament.Backend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "CompeTournament.Backend.dll"]
