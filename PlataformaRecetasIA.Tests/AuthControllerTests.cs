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
using Xunit;

namespace PlataformaRecetasIA.Tests
{
    public class AuthControllerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IResponseCookies> _cookiesMock;
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

            // Configurar mock de HttpResponse
            var responseMock = new Mock<HttpResponse>();
            responseMock.Setup(r => r.Cookies).Returns(_cookiesMock.Object);

            // Configurar HttpContext con el Response mockeado
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(h => h.Response).Returns(responseMock.Object);

            // Inicializar controlador con el HttpContext mockeado
            _controller = new AuthController(_configurationMock.Object, _context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext.Object
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
    }
}