# ──────────────────────────────────────────────────────────────────────
# Stage 1 : Build
# Image SDK lourde, compile tous les projets et publie les binaires
# ──────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copier d'abord les .csproj pour profiter du cache Docker sur dotnet restore
COPY ["src/PriceWatch.Api/PriceWatch.Api.csproj", "src/PriceWatch.Api/"]
COPY ["src/PriceWatch.SharedKernel/PriceWatch.SharedKernel.csproj", "src/PriceWatch.SharedKernel/"]
COPY ["src/Modules/PriceWatch.Modules.Prices/PriceWatch.Modules.Prices.csproj", "src/Modules/PriceWatch.Modules.Prices/"]
COPY ["src/Modules/PriceWatch.Modules.History/PriceWatch.Modules.History.csproj", "src/Modules/PriceWatch.Modules.History/"]

RUN dotnet restore "src/PriceWatch.Api/PriceWatch.Api.csproj"

# Copier tout le code source et publier
COPY src/ src/
RUN dotnet publish "src/PriceWatch.Api/PriceWatch.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ──────────────────────────────────────────────────────────────────────
# Stage 2 : Runtime
# Image ASP.NET Core Runtime (~100 MB), beaucoup plus légère que le SDK
# ──────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Variables d'environnement par défaut (overridables au runtime)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PriceWatch.Api.dll"]
