# Unique build ID: 20251125-002
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out
RUN echo "=== ALL FILES IN /app/out ===" && find /app/out -type f && echo "=== DIRECTORIES ===" && find /app/out -type d

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
RUN echo "=== Final /app ===" && find /app -type f | head -20
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JewelChitApplication.dll"]