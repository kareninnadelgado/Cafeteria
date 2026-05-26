# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiamos el archivo del proyecto y restauramos dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiamos el resto del código y lo compilamos para producción
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa de ejecución (imagen más ligera)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

# Le decimos a Render qué puerto usar
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Comando para encender la app
ENTRYPOINT ["dotnet", "Cafeteria.dll"]
