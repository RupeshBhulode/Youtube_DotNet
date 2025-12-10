# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj first for cache
COPY Web/Web.csproj ./Web/

# restore packages (will use SDK 10)
RUN dotnet restore Web/Web.csproj

# copy the rest
COPY . .

# publish
RUN dotnet publish Web/Web.csproj -c Release -o /app/publish

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Do NOT expand $PORT at build time. Use entrypoint to bind to runtime $PORT.
ENTRYPOINT ["sh", "-c", "dotnet Web.dll --urls \"http://*:${PORT:-5000}\""]
