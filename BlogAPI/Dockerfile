# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BlogAPI/BlogAPI.Web.csproj", "BlogAPI/"]
COPY ["BlogAPI.Application/BlogAPI.Application.csproj", "BlogAPI.Application/"]
COPY ["BlogAPI.Domain/BlogAPI.Domain.csproj", "BlogAPI.Domain/"]
COPY ["BlogAPI.Infrastructure/BlogAPI.Infrastructure.csproj", "BlogAPI.Infrastructure/"]
RUN dotnet restore "BlogAPI/BlogAPI.Web.csproj"
COPY . .
WORKDIR "/src/BlogAPI"
RUN dotnet build "BlogAPI.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BlogAPI.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogAPI.Web.dll"]
