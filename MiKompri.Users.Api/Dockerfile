FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["MiKompri.Users.Api/MiKompri.Users.Api.csproj", "MiKompri.Users.Api/"]
COPY ["MiKompri.Users.Application/MiKompri.Users.Application.csproj", "MiKompri.Users.Application/"]
COPY ["MiKompri.Users.Domain/MiKompri.Users.Domain.csproj", "MiKompri.Users.Domain/"]
COPY ["MiKompri.Users.Infrastructure/MiKompri.Users.Infrastructure.csproj", "MiKompri.Users.Infrastructure/"]
COPY ["MiKompri.ShoppingList.Application/MiKompri.ShoppingList.Application.csproj", "MiKompri.ShoppingList.Application/"]
COPY ["MiKompri.ShoppingList.Domain/MiKompri.ShoppingList.Domain.csproj", "MiKompri.ShoppingList.Domain/"]

RUN dotnet restore "./MiKompri.Users.Api/MiKompri.Users.Api.csproj"
COPY . .
WORKDIR "/src/MiKompri.Users.Api"
RUN dotnet build "./MiKompri.Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MiKompri.Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiKompri.Users.Api.dll"]
