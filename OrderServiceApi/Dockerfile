# Stage 1: Build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source
COPY OrderServiceApi/*.csproj OrderServiceApi/
RUN dotnet restore OrderServiceApi/OrderServiceApi.csproj
COPY OrderServiceApi/. ./OrderServiceApi/
WORKDIR /source/OrderServiceApi
RUN dotnet publish -c Release -o /app --no-restore

# Final stage (final image)

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "OrderServiceApi.dll"]