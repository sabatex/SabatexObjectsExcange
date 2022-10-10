#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebApi1C8Exchange/WebApiDocumentsExchange.csproj", "WebApi1C8Exchange/"]
COPY ["WebApi1C8Exchange.Models/WebApiDocumentsExchange.Models.csproj", "WebApi1C8Exchange.Models/"]
RUN dotnet restore "WebApi1C8Exchange/WebApiDocumentsExchange.csproj"
COPY . .
WORKDIR "/src/WebApi1C8Exchange"
RUN dotnet build "WebApiDocumentsExchange.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApiDocumentsExchange.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApiDocumentsExchange.dll"]