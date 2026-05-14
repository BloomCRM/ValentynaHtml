# =========================================================
# Multi-stage build — ValentynaWeb (.NET 8 Razor Pages)
# =========================================================

# ------- Stage 1: build -------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо csproj окремо — кешування шару залежностей
COPY src/ValentynaWeb/ValentynaWeb.csproj ./ValentynaWeb/
RUN dotnet restore ./ValentynaWeb/ValentynaWeb.csproj

# Копіюємо весь код і публікуємо
COPY src/ValentynaWeb/ ./ValentynaWeb/
WORKDIR /src/ValentynaWeb
RUN dotnet publish -c Release -o /app/publish --no-restore

# ------- Stage 2: runtime -------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Непривілейований користувач
RUN addgroup --gid 1001 appgroup && \
    adduser  --uid 1001 --gid 1001 --disabled-password --gecos "" appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser

# ASP.NET за замовчуванням слухає 8080 всередині контейнера
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "ValentynaWeb.dll"]
