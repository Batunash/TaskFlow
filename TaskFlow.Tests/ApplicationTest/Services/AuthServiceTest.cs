using System;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Identity; 
using Xunit;

namespace TaskFlow.Tests.ApplicationTest.Services
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IPasswordHash> _mockPasswordHasher;
        private readonly Mock<IValidator<LoginDto>> _mockLoginVal;
        private readonly Mock<IValidator<RegisterDto>> _mockRegisterVal;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly AuthService _authService;

        public AuthServiceTest()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHash>();
            _mockLoginVal = new Mock<IValidator<LoginDto>>();
            _mockRegisterVal = new Mock<IValidator<RegisterDto>>();
            var jwtSettings = new JwtSettings
            {
                Secret = "bu-test-icin-cok-uzun-ve-guvenli-bir-secret-key-olmali-123456", 
                Issuer = "TaskFlowTest",
                Audience = "TaskFlowTestUsers",
                ExpirationMinutes = 60
            };

            _tokenGenerator = new JwtTokenGenerator(jwtSettings);
            _authService = new AuthService(
                _mockPasswordHasher.Object,
                _tokenGenerator,
                _mockUserRepo.Object,
                _mockRegisterVal.Object,
                _mockLoginVal.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_Should_Return_Token_When_Valid()
        {
            // Arrange
            var dto = new RegisterDto { UserName = "newuser", Password = "Password123!" };

            _mockRegisterVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ValidationResult());

            _mockUserRepo.Setup(r => r.GetByUserNameAsync(dto.UserName))
                         .ReturnsAsync((User?)null);

            _mockPasswordHasher.Setup(h => h.Hash(dto.Password)).Returns("hashed_secret");

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.UserName, result.UserName);
            Assert.False(string.IsNullOrEmpty(result.AccessToken)); 
            _mockUserRepo.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.UserName == dto.UserName &&
                u.PasswordHash == "hashed_secret"
            )), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_Should_Throw_BusinessRule_When_Username_Taken()
        {
            // Arrange
            var dto = new RegisterDto { UserName = "existing", Password = "Pw" };

            _mockRegisterVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new ValidationResult());

            // Kullanıcı zaten var
            var existingUser = new User("existing", "hash");
            _mockUserRepo.Setup(r => r.GetByUserNameAsync(dto.UserName))
                         .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_Should_Return_Token_When_Credentials_Correct()
        {
            // Arrange
            var dto = new LoginDto { UserName = "user", Password = "Password123!" };
            var user = new User("user", "real_hash", 1); 
            typeof(User).GetProperty("Id")?.SetValue(user, 100);
            _mockLoginVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());
            _mockUserRepo.Setup(r => r.GetByUserNameAsync(dto.UserName)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.Verify(dto.Password, "real_hash")).Returns(true);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.UserId);
            Assert.Equal(1, result.OrganizationId);
            Assert.False(string.IsNullOrEmpty(result.AccessToken)); 
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_When_User_NotFound()
        {
            // Arrange
            var dto = new LoginDto { UserName = "ghost", Password = "Pw" };

            _mockLoginVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            _mockUserRepo.Setup(r => r.GetByUserNameAsync(dto.UserName)).ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _authService.LoginAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_When_Password_Wrong()
        {
            // Arrange
            var dto = new LoginDto { UserName = "user", Password = "WrongPassword" };
            var user = new User("user", "real_hash");

            _mockLoginVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            _mockUserRepo.Setup(r => r.GetByUserNameAsync(dto.UserName)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.Verify(dto.Password, "real_hash")).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _authService.LoginAsync(dto));
        }
    }
}