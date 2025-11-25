ARG BUILDKIT_INLINE_CACHE=1
ARG BUILD_ID=20251125-005

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out
RUN echo "=== CONTENTS ===" && ls -lh /app/out/ && echo "=== TOTAL FILES ===" && find /app/out -type f | wc -l

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
RUN ls -lh /app/ | head -20
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JewelChitApplication.dll"]