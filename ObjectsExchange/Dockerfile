#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApiDocumentsExcange/ObjectsExchange/ObjectsExchange.csproj", "WebApiDocumentsExcange/ObjectsExchange/"]
COPY ["WebApiDocumentsExcange/ObjectsExchange.Client/ObjectsExchange.Client.csproj", "WebApiDocumentsExcange/ObjectsExchange.Client/"]
COPY ["Sabatex.RadzenBlazor/Sabatex.RadzenBlazor/Sabatex.RadzenBlazor.csproj", "Sabatex.RadzenBlazor/Sabatex.RadzenBlazor/"]
COPY ["WebApiDocumentsExcange/src/Sabatex.ObjectsExchange.Models/Sabatex.ObjectsExchange.Models.csproj", "WebApiDocumentsExcange/src/Sabatex.ObjectsExchange.Models/"]
RUN dotnet restore "./WebApiDocumentsExcange/ObjectsExchange/ObjectsExchange.csproj"
COPY . .
WORKDIR "/src/WebApiDocumentsExcange/ObjectsExchange"
RUN dotnet build "./ObjectsExchange.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ObjectsExchange.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ObjectsExchange.dll"]