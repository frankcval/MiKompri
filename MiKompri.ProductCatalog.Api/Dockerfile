# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MiKompri.ProductCatalog.Api/MiKompri.ProductCatalog.Api.csproj", "MiKompri.ProductCatalog.Api/"]
COPY ["MiKompri.ProductCatalog.Application/MiKompri.ProductCatalog.Application.csproj", "MiKompri.ProductCatalog.Application/"]
COPY ["MiKompri.ProductCatalog.Domain/MiKompri.ProductCatalog.Domain.csproj", "MiKompri.ProductCatalog.Domain/"]
COPY ["MiKompri.ProductCatalog.Infrastructure/MiKompri.ProductCatalog.Infrastructure.csproj", "MiKompri.ProductCatalog.Infrastructure/"]
RUN dotnet restore "./MiKompri.ProductCatalog.Api/MiKompri.ProductCatalog.Api.csproj"
COPY . .
WORKDIR "/src/MiKompri.ProductCatalog.Api"
RUN dotnet build "./MiKompri.ProductCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MiKompri.ProductCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiKompri.ProductCatalog.Api.dll"]
