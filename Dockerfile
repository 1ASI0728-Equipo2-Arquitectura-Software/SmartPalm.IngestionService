FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore SmartPalm.IngestionService.slnx
RUN dotnet publish SmartPalm.IngestionService.API/SmartPalm.IngestionService.API.csproj -c Release -o /app/publish --no-restore
FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=10 \
  CMD bash -lc 'exec 3<>/dev/tcp/127.0.0.1/8080'
ENTRYPOINT ["dotnet", "SmartPalm.IngestionService.API.dll"]
