using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PlataformaRecetasIA.Controllers;
using PlataformaRecetasIA.Data;
using PlataformaRecetasIA.Models;
using System;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace PlataformaRecetasIA.Tests
{
    public class AuthControllerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IResponseCookies> _cookiesMock;
        private readonly Mock<HttpResponse> _responseMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            // Configurar base de datos en memoria
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            // Configurar mock de IConfiguration
            _configurationMock = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s.Value).Returns("YourSuperSecretKeyWithAtLeast32CharactersForHMACSHA256");
            _configurationMock.Setup(c => c.GetSection("Jwt:SecretKey")).Returns(configurationSectionMock.Object);
            _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns("YourSuperSecretKeyWithAtLeast32CharactersForHMACSHA256");

            // Configurar mock de IResponseCookies
            _cookiesMock = new Mock<IResponseCookies>();
            _cookiesMock.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            _cookiesMock.Setup(c => c.Delete(It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();

            // Configurar mock de HttpResponse
            _responseMock = new Mock<HttpResponse>();
            _responseMock.Setup(r => r.Cookies).Returns(_cookiesMock.Object);

            // Configurar mock de HttpContext
            _httpContextMock = new Mock<HttpContext>();
            _httpContextMock.Setup(h => h.Response).Returns(_responseMock.Object);

            // Inicializar controlador
            _controller = new AuthController(_configurationMock.Object, _context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContextMock.Object
                }
            };
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsRedirectToRecetasIndex()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword"),
                CanUseAI = 0
            };
            _context.Usuarios.Add(user);
            _context.SaveChanges();

            var userLogin = new UserLogin
            {
                Username = "testuser",
                Password = "testpassword"
            };

            // Act
            var result = _controller.Login(userLogin) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Index");
            result!.ControllerName.Should().Be("Recetas");

            // Verificar que se creó la cookie con el token JWT
            _cookiesMock.Verify(c => c.Append("JwtToken", It.IsAny<string>(), It.IsAny<CookieOptions>()), Times.Once());
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var userLogin = new UserLogin
            {
                Username = "nonexistent",
                Password = "wrongpassword"
            };

            // Act
            var result = _controller.Login(userLogin) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.ViewData["Error"].Should().Be("Credenciales inválidas");
        }

        [Fact]
        public void Register_ValidCredentials_CreatesUserAndRedirectsToLogin()
        {
            // Arrange
            var userLogin = new UserLogin
            {
                Username = "newuser",
                Password = "newpassword"
            };

            // Act
            var result = _controller.Register(userLogin) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Login");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Username == "newuser");
            usuario.Should().NotBeNull();
            usuario!.Username.Should().Be("newuser");
            usuario!.CanUseAI.Should().Be(0);
            BCrypt.Net.BCrypt.Verify(userLogin.Password, usuario.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public void Register_ExistingUsername_ReturnsViewWithError()
        {
            // Arrange
            var existingUser = new Usuario
            {
                Id = 1,
                Username = "existinguser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                CanUseAI = 0
            };
            _context.Usuarios.Add(existingUser);
            _context.SaveChanges();

            var userLogin = new UserLogin
            {
                Username = "existinguser",
                Password = "newpassword"
            };

            // Act
            var result = _controller.Register(userLogin) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.ViewData["Error"].Should().Be("El nombre de usuario ya existe.");
        }

        [Fact]
        public void Logout_RemovesJwtTokenAndRedirectsToLogin()
        {
            // Act
            var result = _controller.Logout() as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.ActionName.Should().Be("Login");

            // Verificar que se eliminó la cookie
            _cookiesMock.Verify(c => c.Delete("JwtToken", It.IsAny<CookieOptions>()), Times.Once());
        }

        [Fact]
        public void CheckAuth_AuthenticatedUserWithAICapability_ReturnsCorrectJson()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword"),
                CanUseAI = 1
            };
            _context.Usuarios.Add(user);
            _context.SaveChanges();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(h => h.User).Returns(principal);

            // Act
            var result = _controller.CheckAuth() as JsonResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().BeEquivalentTo(new { authenticated = true, canUseAI = true });
        }

        [Fact]
        public void CheckAuth_AuthenticatedUserWithoutAICapability_ReturnsCorrectJson()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword"),
                CanUseAI = 0
            };
            _context.Usuarios.Add(user);
            _context.SaveChanges();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(h => h.User).Returns(principal);

            // Act
            var result = _controller.CheckAuth() as JsonResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().BeEquivalentTo(new { authenticated = true, canUseAI = false });
        }

        [Fact]
        public void CheckAuth_UnauthenticatedUser_ReturnsCorrectJson()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Sin claims, no autenticado
            var principal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(h => h.User).Returns(principal);
            _httpContextMock.Setup(h => h.User.Identity.IsAuthenticated).Returns(false);

            // Act
            var result = _controller.CheckAuth() as JsonResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().BeEquivalentTo(new { authenticated = false, canUseAI = false });
        }
    }
}