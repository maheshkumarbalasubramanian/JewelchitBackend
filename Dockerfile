ARG BUILD_ID=20251125-006

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
RUN echo "=== FILES IN /app ===" && find /app -maxdepth 1 -type f | wc -l && echo "Main app DLL exists:" && test -f /app/JewelChitApplication.dll && echo "YES" || echo "NO"
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JewelChitApplication.dll"]