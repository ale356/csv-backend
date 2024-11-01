# Use the official .NET runtime for .NET 8 as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the .NET SDK image for .NET 8 to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["csv-backend.csproj", "./"]
RUN dotnet restore "csv-backend.csproj"

# Copy and publish the app
COPY . .
RUN dotnet publish "csv-backend.csproj" -c Release -o /app/publish

# Use the runtime image to run the app
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "csv-backend.dll"]
