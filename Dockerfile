# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy
COPY certifyab.csproj ./

# Restore
RUN dotnet restore ./certifyab.csproj

# Copy remaining project
COPY . .

# Build & Publish
RUN dotnet publish ./certifyab.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published app from build
COPY --from=build /app/publish .


# Container app will listen on port 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Start API
ENTRYPOINT ["dotnet", "certifyab.dll"]