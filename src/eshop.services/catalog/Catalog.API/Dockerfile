FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 6060

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eshop.services/catalog/Catalog.API/Catalog.API.csproj", "eshop.services/catalog/Catalog.API/"]
RUN dotnet restore "eshop.services/catalog/Catalog.API/Catalog.API.csproj"
COPY . .
WORKDIR "/src/eshop.services/catalog/Catalog.API"
RUN apt-get update && apt-get install -y protobuf-compiler && dotnet build "./Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:Protobuf_ProtocFullPath=`which protoc`

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
