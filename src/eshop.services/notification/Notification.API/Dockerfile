FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 6064

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eshop.services/notification/Notification.API/Notification.API.csproj", "eshop.services/notification/Notification.API/"]
COPY ["eshop.shared/blocks/BuildingBlocks/BuildingBlocks.csproj", "eshop.shared/blocks/BuildingBlocks/"]
COPY ["eshop.shared/blocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "eshop.shared/blocks/BuildingBlocks.Messaging/"]
RUN dotnet restore "eshop.services/notification/Notification.API/Notification.API.csproj"
COPY . .
WORKDIR "/src/eshop.services/notification/Notification.API"
RUN dotnet build "./Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
