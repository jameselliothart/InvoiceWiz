FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Contracts and Persister csproj files, then restore
COPY Contracts/*.csproj ./Contracts/
COPY Persister/*.csproj ./Persister/
RUN dotnet restore Persister/Persister.csproj

# Copy source code and publish
COPY Contracts/ ./Contracts/
COPY Persister/ ./Persister/
RUN dotnet publish Persister/Persister.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "Persister.dll"]