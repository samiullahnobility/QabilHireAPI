FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY NuGet.Config ./
COPY src/QabilHire.Api/QabilHire.Api.csproj src/QabilHire.Api/
COPY src/QabilHire.Application/QabilHire.Application.csproj src/QabilHire.Application/
COPY src/QabilHire.Domain/QabilHire.Domain.csproj src/QabilHire.Domain/
COPY src/QabilHire.Infrastructure/QabilHire.Infrastructure.csproj src/QabilHire.Infrastructure/
RUN dotnet restore src/QabilHire.Api/QabilHire.Api.csproj

COPY src/ src/
RUN dotnet publish src/QabilHire.Api/QabilHire.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
CMD ["sh", "-c", "dotnet QabilHire.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
