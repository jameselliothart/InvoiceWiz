# Use the official .NET 8 SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all csproj files and restore
COPY Contracts/*.csproj ./Contracts/
COPY APIGateway/*.csproj ./APIGateway/
# Restores Contracts too since it's referenced
RUN dotnet restore APIGateway/APIGateway.csproj

# Copy all source and publish
COPY Contracts/ ./Contracts/
COPY APIGateway/ ./APIGateway/
RUN dotnet publish APIGateway/APIGateway.csproj -c Release -o out

# Use the official ASP.NET Core Runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "APIGateway.dll"]
