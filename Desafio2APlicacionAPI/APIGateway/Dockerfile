# Establece la imagen base para .NET 8.0 (ASP.NET Core Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Establece la imagen base para .NET SDK 8.0 (para compilar la aplicación)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["APIGateway.csproj", "./"]
RUN dotnet restore "./APIGateway.csproj"

# Copia el resto de los archivos del proyecto y compila
COPY . .
RUN dotnet build "APIGateway.csproj" -c Release -o /app/build

# Publica la aplicación
RUN dotnet publish "APIGateway.csproj" -c Release -o /app/publish

# Establece la imagen final basada en ASP.NET Core para ejecutar la aplicación
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "APIGateway.dll"]
