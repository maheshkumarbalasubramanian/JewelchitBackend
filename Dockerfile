# Unique build ID: 20251125-001
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN echo "Building..."
RUN find . -name "*.csproj"
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out
RUN echo "Publish complete - checking output:"
RUN ls -la /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
RUN echo "Final /app contents:"
RUN ls -la /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JewelChitApplication.dll"]