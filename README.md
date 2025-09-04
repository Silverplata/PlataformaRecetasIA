# Plataforma de Recetas con IA

## Descripción

**Plataforma de Recetas con IA** es una aplicación web full-stack desarrollada en .NET Core que permite a los usuarios gestionar recetas culinarias, importar recetas desde archivos XML, calcular porciones ajustadas, y generar recetas nuevas utilizando la API de OpenAI. La aplicación implementa autenticación basada en JWT, formularios con jQuery y Ajax, y se despliega fácilmente en una plataforma como Render utilizando Docker. La base de datos utiliza MySQL (Aiven) para almacenar usuarios y recetas.

### Características principales
- **Autenticación segura**: Registro e inicio de sesión con JWT, con menús protegidos que se muestran solo a usuarios autenticados.
- **Gestión de recetas**: CRUD completo (Crear, Leer, Actualizar, Eliminar) para recetas, con soporte para ingredientes y descripciones.
- **Importación de recetas**: Carga de recetas desde archivos XML.
- **Búsqueda de recetas**: Filtrado de recetas por nombre o descripción.
- **Calculadora de porciones**: Ajuste dinámico de cantidades de ingredientes según el número de porciones.
- **Generación de recetas con IA**: Creación de recetas personalizadas usando la API de OpenAI, basada en los ingredientes proporcionados.
- **Despliegue en Docker**: Configuración lista para desplegar en plataformas gratuitas como Render.

## Tecnologías utilizadas
- **Backend**: .NET Core 8.0, Entity Framework Core, MySQL (Aiven)
- **Frontend**: ASP.NET MVC, jQuery, Ajax, Bootstrap
- **Autenticación**: JWT (JSON Web Tokens) con cookies HttpOnly
- **Integración con IA**: API de OpenAI (modelo `gpt-3.5-turbo`)
- **Despliegue**: Docker, Render
- **Formato de datos**: XML para importación de recetas

## Requisitos previos
- **.NET SDK 8.0**: Instalar desde [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
- **MySQL (Aiven)**: Cuenta en Aiven para la base de datos (o un servidor MySQL local)
- **Docker**: Para el despliegue en Render ([https://www.docker.com/get-started](https://www.docker.com/get-started))
- **Visual Studio 2022** (o superior) o cualquier IDE compatible con .NET
- **Node.js** (opcional, para herramientas de frontend adicionales)
- **Clave de API de OpenAI**: Obtener desde [https://platform.openai.com/](https://platform.openai.com/)

## Instalación y configuración

1. **Clonar el repositorio**:
   ```bash
   git clone <tu-repositorio-git>
   cd PlataformaRecetasIA
   ```

2. **Configurar la base de datos**:
   - Crea una base de datos en Aiven MySQL o un servidor MySQL local.
   - Actualiza la cadena de conexión en `appsettings.json`:
     ```json
     "ConnectionStrings": {
         "DefaultConnection": "Server=<host>;Port=<puerto>;Database=RecetasDB;User Id=<usuario>;Password=<contraseña>;SslMode=Required;"
     }
     ```

3. **Configurar claves de API y JWT**:
   - Actualiza `appsettings.json` con tu clave de OpenAI y la clave secreta JWT:
     ```json
     "Jwt": {
         "SecretKey": "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"
     },
     "OpenAI": {
         "ApiKey": "<tu-clave-openai>"
     }
     ```

4. **Restaurar dependencias**:
   ```bash
   dotnet restore
   ```

5. **Aplicar migraciones a la base de datos**:
   - Asegúrate de que el paquete `Microsoft.EntityFrameworkCore.Design` esté instalado.
   - Ejecuta:
     ```bash
     dotnet ef migrations add InitialCreate
     dotnet ef database update
     ```

6. **Ejecutar la aplicación localmente**:
   - Desde Visual Studio: Presiona `F5` (asegúrate de que el proyecto esté configurado para ejecutarse en `https://localhost:32779`).
   - Desde la terminal:
     ```bash
     dotnet run
     ```

## Estructura del proyecto
```
PlataformaRecetasIA/
├── Controllers/
│   ├── AuthController.cs       # Maneja autenticación (login, registro, logout)
│   ├── RecetasController.cs    # Maneja CRUD de recetas, búsqueda, porciones, IA
├── Models/
│   ├── Receta.cs               # Modelo para recetas
│   ├── Ingrediente.cs          # Modelo para ingredientes
│   ├── UserLogin.cs            # Modelo para login/registro
│   ├── Usuario.cs              # Modelo para usuarios
├── Data/
│   ├── AppDbContext.cs         # Contexto de Entity Framework para MySQL
├── Views/
│   ├── Auth/
│   │   ├── Login.cshtml        # Vista para iniciar sesión
│   │   ├── Register.cshtml     # Vista para registrarse
│   ├── Recetas/
│   │   ├── Index.cshtml        # Lista de recetas
│   │   ├── Create.cshtml       # Formulario para agregar recetas
│   │   ├── Search.cshtml       # Formulario de búsqueda
│   │   ├── Portions.cshtml     # Calculadora de porciones
│   │   ├── Generar.cshtml      # Generación de recetas con IA
│   ├── Shared/
│   │   ├── _Layout.cshtml      # Diseño principal con menú dinámico
├── wwwroot/
│   ├── css/                    # Estilos CSS
│   ├── js/                     # Scripts JavaScript (jQuery, Ajax)
├── appsettings.json            # Configuración de conexión, JWT, y OpenAI
├── Program.cs                  # Configuración del middleware (JWT, MVC)
├── Dockerfile                  # Configuración para Docker
├── render.yaml                 # Configuración para despliegue en Render
```

## Uso

1. **Registro e inicio de sesión**:
   - Navega a `/Auth/Register` para crear un usuario (por ejemplo, `peter/peter123`).
   - Inicia sesión en `/Auth/Login`. Tras el login, se genera una cookie `JwtToken` y los menús de recetas se muestran.

2. **Gestión de recetas**:
   - `/Recetas`: Lista todas las recetas.
   - `/Recetas/Create`: Agrega una nueva receta.
   - `/Recetas/Edit/<id>`: Edita una receta existente.
   - `/Recetas/Delete/<id>`: Elimina una receta.
   - `/Recetas/ImportarXml`: Sube un archivo XML para importar recetas.

3. **Búsqueda**:
   - `/Recetas/Search`: Busca recetas por nombre o descripción.

4. **Calculadora de porciones**:
   - `/Recetas/Portions`: Selecciona una receta y ajusta las porciones para recalcular las cantidades de ingredientes.

5. **Generación de recetas con IA**:
   - `/Recetas/Generar`: Ingresa ingredientes (por ejemplo, "tomate, queso, harina") y genera una receta usando la API de OpenAI.

6. **Cerrar sesión**:
   - Haz clic en "Cerrar Sesión" (`/Auth/Logout`) para eliminar la cookie `JwtToken` y ocultar los menús de recetas.

## Despliegue en Render

1. **Crear un repositorio Git**:
   - Sube el proyecto a GitHub, GitLab, o similar.

2. **Configurar `render.yaml`**:
   ```yaml
   services:
       - type: web
         name: plataformarecetas
         env: docker
         plan: free
         repo: <tu-repositorio-git>
         healthCheckPath: /
         envVars:
           - key: ASPNETCORE_ENVIRONMENT
             value: Production
           - key: ConnectionStrings__DefaultConnection
             value: Server=xxxxxxxxxxxxxxx;Port=18276;Database=RecetasDB;User Id=xxxx;Password=xxxxxxxxxxxxxx;SslMode=Required;
           - key: OpenAI__ApiKey
             value: <tu-api-key-openai>
           - key: Jwt__SecretKey
             value: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
   ```

3. **Verificar `Dockerfile`**:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0
   COPY . /app
   WORKDIR /app
   COPY appsettings.json .
   ENTRYPOINT ["dotnet", "PlataformaRecetasIA.dll"]
   ```

4. **Desplegar en Render**:
   - Crea un nuevo servicio web en [Render](https://render.com/).
   - Selecciona Docker como entorno y conecta tu repositorio.
   - Configura las variables de entorno según `render.yaml`.
   - Despliega y accede a la URL generada (por ejemplo, `https://plataformarecetas.onrender.com`).

## Depuración

- **Logs del servidor**:
  - Revisa la ventana **Output** en Visual Studio (o los logs en la consola) para mensajes como:
    - `Fallo de autenticación: ...` (para problemas con JWT)
    - `Iniciando Generar con ingredientes: ...` (para problemas con la API de OpenAI)
  - Habilita el nivel de log `Information` en `appsettings.json`.

- **Consola del navegador**:
  - Abre F12 > Console para verificar mensajes de error en las solicitudes Ajax (por ejemplo, en `/Recetas/Generar`).
  - Busca `Token CSRF:` y `Estado de autenticación:` para confirmar que las solicitudes se envían correctamente.

- **Cookie JWT**:
  - Verifica la cookie `JwtToken` en F12 > Application > Cookies tras el login.
  - Decodifica el token en [jwt.io](https://jwt.io/) para confirmar que contiene el nombre de usuario y una fecha de expiración válida.

- **Errores comunes**:
  - **401 Unauthorized**: Asegúrate de que el token JWT sea válido y que la clave en `appsettings.json` coincida con la usada en `Program.cs`.
  - **400 Bad Request en `/Recetas/Generar`**: Verifica que la clave de OpenAI sea válida y que los ingredientes no estén vacíos.
  - **Menús no se muestran**: Revisa la respuesta de `/Auth/CheckAuth` en la consola del navegador.

## Contribuir
1. Clona el repositorio y crea una rama para tu funcionalidad:
   ```bash
   git checkout -b mi-funcionalidad
   ```
2. Realiza tus cambios y haz un commit:
   ```bash
   git commit -m "Descripción de los cambios"
   ```
3. Sube la rama y crea un pull request:
   ```bash
   git push origin mi-funcionalidad
   ```

## Licencia
Este proyecto está bajo la licencia MIT. Consulta el archivo `LICENSE` para más detalles.

## Contacto
Para dudas o sugerencias, contacta a través del repositorio o por correo electrónico a `<tu-correo>`.