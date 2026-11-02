using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Configuration.Validation;
using Microsoft.Extensions.Options;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests
{
    public class ServerOptionsValidatorTests
    {
        [Fact]
        public void Validate_WithValidPort_ReturnsSuccess()
        {
            // Arrange
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions
            {
                Port = 5000,
                MaxClients = 10
            };

            // Act
            var result = validator.Validate("Server", options);

            // Assert
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithValidMaxClients_ReturnsSuccess()
        {
            // Arrange
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions
            {
                Port = 5000,
                MaxClients = 5
            };

            // Act
            var result = validator.Validate("Server", options);

            // Assert
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(65536)]
        [InlineData(-5000)]
        public void Validate_WithInvalidPort_ReturnsFail(int invalidPort)
        {
            // Arrange
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions
            {
                Port = invalidPort,
                MaxClients = 10
            };

            // Act
            var result = validator.Validate("Server", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            Assert.NotEmpty(result.Failures);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(0)]
        public void Validate_WithInvalidMaxClients_ReturnsFail(int invalidMaxClients)
        {
            // Arrange
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions
            {
                Port = 5000,
                MaxClients = invalidMaxClients
            };

            // Act
            var result = validator.Validate("Server", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            Assert.NotEmpty(result.Failures);
        }

        [Fact]
        public void Validate_WithBoundaryPortValue_ReturnsSuccess()
        {
            // Arrange
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions
            {
                Port = 1,
                MaxClients = 1
            };

            // Act
            var result = validator.Validate("Server", options);

            // Assert
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithHighMaxClients_ReturnsSuccess()
        {
            // Arrange
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions
            {
                Port = 5000,
                MaxClients = 1000
            };

            // Act
            var result = validator.Validate("Server", options);

            // Assert
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_ErrorMessageIsClear_OnInvalidPort()
        {
            // Arrange
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions
            {
                Port = -1,
                MaxClients = 10
            };

            // Act
            var result = validator.Validate("Server", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            var failures = result.Failures.ToList();
            Assert.NotEmpty(failures);
            Assert.True(failures.Any(f => f.Contains("Port", StringComparison.OrdinalIgnoreCase)), 
                "Error message should mention Port");
        }

        [Fact]
        public void Validate_ErrorMessageIsClear_OnInvalidMaxClients()
        {
            // Arrange
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions
            {
                Port = 5000,
                MaxClients = -1
            };

            // Act
            var result = validator.Validate("Server", options);

            // Assert
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            var failures = result.Failures.ToList();
            Assert.NotEmpty(failures);
            Assert.True(failures.Any(f => f.Contains("MaxClients", StringComparison.OrdinalIgnoreCase) || 
                                          f.Contains("max", StringComparison.OrdinalIgnoreCase)),
                "Error message should mention MaxClients or max");
        }
    }
}
