# Use the official .NET runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CsvUploadApp.csproj", "./"]
RUN dotnet restore "CsvUploadApp.csproj"

# Copy and publish the app
COPY . .
RUN dotnet publish "CsvUploadApp.csproj" -c Release -o /app/publish

# Use the base image to run the app
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CsvUploadApp.dll"]
