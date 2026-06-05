FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["IndexerWorker/IndexerWorker.csproj", "IndexerWorker/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "IndexerWorker/IndexerWorker.csproj"

COPY . .
WORKDIR "/src/IndexerWorker"
RUN dotnet build "IndexerWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IndexerWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IndexerWorker.dll"]
