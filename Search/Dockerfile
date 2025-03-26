FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Contracts and Search csproj files, then restore
COPY Contracts/*.csproj ./Contracts/
COPY Search/*.csproj ./Search/
RUN dotnet restore Search/Search.csproj

# Copy protobuf definitions
COPY protos ./protos/

# Copy source code and publish
COPY Contracts/ ./Contracts/
COPY Search/ ./Search/
RUN dotnet publish Search/Search.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Search.dll"]