# Unique build ID: 20251125-004
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out
RUN echo "=== CHECKING /app/out CONTENTS ===" && ls -lh /app/out/ | head -30

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
RUN echo "=== CHECKING /app AFTER COPY ===" && ls -lh /app | head -30
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JewelChitApplication.dll"]