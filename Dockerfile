FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY HotelBooking.sln .
COPY src/HotelBooking.Api/HotelBooking.Api.csproj src/HotelBooking.Api/
COPY src/HotelBooking.Application/HotelBooking.Application.csproj src/HotelBooking.Application/
COPY src/HotelBooking.Domain/HotelBooking.Domain.csproj src/HotelBooking.Domain/
COPY src/HotelBooking.Infrastructure/HotelBooking.Infrastructure.csproj src/HotelBooking.Infrastructure/
COPY tests/HotelBooking.Tests/HotelBooking.Tests.csproj tests/HotelBooking.Tests/
RUN dotnet restore src/HotelBooking.Api/HotelBooking.Api.csproj

COPY src/ src/
RUN dotnet publish src/HotelBooking.Api/HotelBooking.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN addgroup --system --gid 1000 appgroup \
    && adduser --system --uid 1000 --ingroup appgroup appuser \
    && mkdir -p /app/data /app/logs \
    && chown -R appuser:appgroup /app

COPY --from=build /app .
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelBooking.Api.dll"]
