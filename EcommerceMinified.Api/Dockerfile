FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["EcommerceMinified.Api/EcommerceMinified.Api.csproj", "EcommerceMinified.Api/"]
COPY ["EcommerceMinified.Application/EcommerceMinified.Application.csproj", "EcommerceMinified.Application/"]
COPY ["EcommerceMinified.Data/EcommerceMinified.Data.csproj", "EcommerceMinified.Data/"]
COPY ["EcommerceMinified.Data.Postgres/EcommerceMinified.Data.Postgres.csproj", "EcommerceMinified.Data.Postgres/"]
COPY ["EcommerceMinified.Data.Rest/EcommerceMinified.Data.Rest.csproj", "EcommerceMinified.Data.Rest/"]
COPY ["EcommerceMinified.Domain/EcommerceMinified.Domain.csproj", "EcommerceMinified.Domain/"]
COPY ["EcommerceMinified.IoC/EcommerceMinified.IoC.csproj", "EcommerceMinified.IoC/"]
COPY ["EcommerceMinified.MsgContracts/EcommerceMinified.MsgContracts.csproj", "EcommerceMinified.MsgContracts/"]

RUN dotnet restore "./EcommerceMinified.Api/EcommerceMinified.Api.csproj"

COPY . .
WORKDIR "/src/EcommerceMinified.Api"
RUN dotnet publish "./EcommerceMinified.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV TZ=America/Sao_Paulo \
    ASPNETCORE_URLS=http://*:8080 \
    ASPNETCORE_ENVIRONMENT=Staging


COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EcommerceMinified.Api.dll"]