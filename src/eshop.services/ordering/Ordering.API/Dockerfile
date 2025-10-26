FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 6063

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eshop.services/ordering/Ordering.API/Ordering.API.csproj", "eshop.services/ordering/Ordering.API/"]
COPY ["eshop.services/ordering/Ordering.Application/Ordering.Application.csproj", "eshop.services/ordering/Ordering.Application/"]
COPY ["eshop.services/ordering/Ordering.Domain/Ordering.Domain.csproj", "eshop.services/ordering/Ordering.Domain/"]
COPY ["eshop.shared/blocks/BuildingBlocks/BuildingBlocks.csproj", "eshop.shared/blocks/BuildingBlocks/"]
COPY ["eshop.shared/blocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "eshop.shared/blocks/BuildingBlocks.Messaging/"]
COPY ["eshop.services/ordering/Ordering.Infrastructure/Ordering.Infrastructure.csproj", "eshop.services/ordering/Ordering.Infrastructure/"]
RUN dotnet restore "eshop.services/ordering/Ordering.API/Ordering.API.csproj"
COPY . .
WORKDIR "/src/eshop.services/ordering/Ordering.API"
RUN dotnet build "./Ordering.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ordering.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ordering.API.dll"]
