# Etapa 1: compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish CoordinacionOrientadores.csproj -c Release -o /app

# Etapa 2: imagen final, más liviana, solo con el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render (y la mayoría de hostings) asignan el puerto por la variable de entorno PORT.
# Si no viene definida (por ejemplo al probar localmente con "docker run"), usa 8080 por defecto.
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} dotnet CoordinacionOrientadores.dll"]
