# Usa la imagen base de .NET SDK para construir el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PlataformaRecetasIA/PlataformaRecetasIA.csproj", "PlataformaRecetasIA/"]
RUN dotnet restore "PlataformaRecetasIA/PlataformaRecetasIA.csproj"
COPY . .
WORKDIR "/src/PlataformaRecetasIA"
RUN dotnet build "PlataformaRecetasIA.csproj" -c Release -o /app/build

# Publicar el proyecto
FROM build AS publish
RUN dotnet publish "PlataformaRecetasIA.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PlataformaRecetasIA.dll"]