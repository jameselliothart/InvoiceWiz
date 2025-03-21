FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Contracts and Generator csproj files, then restore
COPY Contracts/*.csproj ./Contracts/
COPY Generator/*.csproj ./Generator/
RUN dotnet restore Generator/Generator.csproj

# Copy source code and publish
COPY Contracts/ ./Contracts/
COPY Generator/ ./Generator/
RUN dotnet publish Generator/Generator.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "Generator.dll"]