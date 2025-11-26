# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj(s) only to leverage layer caching (adjust path if your csproj inside folder)
COPY ["JewelChitApplication/JewelChitApplication.csproj", "JewelChitApplication/"]
RUN dotnet restore "JewelChitApplication/JewelChitApplication.csproj"

# copy rest of sources
COPY . .
RUN dotnet publish "JewelChitApplication/JewelChitApplication.csproj" -c Release -o /app/out --no-restore -p:UseAppHost=false

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# copy published output from build stage
COPY --from=build /app/out .

# ensure app listens on Railway port
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "JewelChitApplication.dll"]
