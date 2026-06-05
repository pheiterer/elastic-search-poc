FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SearchApi/SearchApi.csproj", "SearchApi/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "SearchApi/SearchApi.csproj"

COPY . .
WORKDIR "/src/SearchApi"
RUN dotnet build "SearchApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SearchApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SearchApi.dll"]
