FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 6065

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eshop.gateway/YarpApiGateway/YarpApiGateway.csproj", "eshop.gateway/YarpApiGateway/"]
RUN dotnet restore "eshop.gateway/YarpApiGateway/YarpApiGateway.csproj"
COPY . .
WORKDIR "/src/eshop.gateway/YarpApiGateway"
RUN dotnet build "./YarpApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./YarpApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YarpApiGateway.dll"]
