# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source
COPY PaymentServiceApi/*.csproj PaymentServiceApi/
RUN dotnet restore PaymentServiceApi/PaymentServiceApi.csproj
COPY PaymentServiceApi/. ./PaymentServiceApi/
WORKDIR /source/PaymentServiceApi
RUN dotnet publish -c Release -o /app --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "PaymentServiceApi.dll"]