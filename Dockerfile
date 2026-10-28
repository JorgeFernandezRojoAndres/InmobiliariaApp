# ===============================
# Dockerfile - InmobiliariaApp (producción, .NET 9.0)
# ===============================
# La aplicación se compila con el SDK de .NET 9 y se ejecuta en una
# imagen slim de ASP.NET (runtime solo) para reducir el peso final.

# Etapa 1: Build con SDK de .NET 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY InmobiliariaApp.csproj .
RUN dotnet restore

# Copiar el resto del código fuente
COPY . .

# Publicar la aplicación en Release
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime de .NET 9 (sin SDK, solo runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copiar el resultado publicado
COPY --from=build /app/publish .

# Explicitar el puerto de escucha
EXPOSE 5027

# Variables de entorno de configuración
# La cadena de conexión se sobreescribe en docker-compose mediante la variable
# ConnectionStrings__DefaultConnection (las appsettings.json de esta fase no se incluyen).
ENV ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=mi_base_datos;User=app_user;Password=app_pass123;SslMode=Preferred;AllowPublicKeyRetrieval=True;ConvertZeroDatetime=True"
ENV Jwt__Key=SuperClaveSecreta_Inmobiliaria_2025_Dev
ENV Jwt__Issuer=InmobiliariaApp
ENV Jwt__Audience=InmobiliariaMobile
ENV Smtp__Host=smtp.gmail.com
ENV Smtp__Port=587
ENV Smtp__User=no-reply@inmobiliaria.com
ENV Smtp__Pass=
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5027

# El punto de entrada de la aplicación
# --urls http://0.0.0.0:5027: escucha en todas las interfaces
ENTRYPOINT ["dotnet", "InmobiliariaApp.dll", "--urls", "http://0.0.0.0:5027"]
