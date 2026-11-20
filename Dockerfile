FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY InventarioApi/InventarioApi.csproj InventarioApi/
RUN dotnet restore InventarioApi/InventarioApi.csproj
COPY . .
RUN dotnet publish InventarioApi/InventarioApi.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventarioApi.dll"]
