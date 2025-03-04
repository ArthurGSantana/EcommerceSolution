FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["HubMinified.Api/HubMinified.Api.csproj", "HubMinified.Api/"]
COPY ["HubMinified.Application/HubMinified.Application.csproj", "HubMinified.Application/"]
COPY ["HubMinified.Data/HubMinified.Data.csproj", "HubMinified.Data/"]
COPY ["HubMinified.Data.Mongo/HubMinified.Data.Mongo.csproj", "HubMinified.Data.Mongo/"]
COPY ["HubMinified.Domain/HubMinified.Domain.csproj", "HubMinified.Domain/"]
COPY ["HubMinified.IoC/HubMinified.IoC.csproj", "HubMinified.IoC/"]
COPY ["HubMinified.MsgContracts/HubMinified.MsgContracts.csproj", "HubMinified.MsgContracts/"]

RUN dotnet restore "./HubMinified.Api/HubMinified.Api.csproj"

COPY . .
WORKDIR "/src/HubMinified.Api"
RUN dotnet publish "./HubMinified.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV TZ=America/Sao_Paulo \
    ASPNETCORE_URLS=http://*:8081 \
    ASPNETCORE_ENVIRONMENT=Staging


COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HubMinified.Api.dll"]