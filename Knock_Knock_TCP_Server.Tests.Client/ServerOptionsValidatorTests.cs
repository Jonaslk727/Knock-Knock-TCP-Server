using System;
using System.Linq;
using Knock_Knock_TCP_Server.Configuration;
using Knock_Knock_TCP_Server.Configuration.Validation;
using Microsoft.Extensions.Options;
using Xunit;

namespace Knock_Knock_TCP_Server.Tests.Client
{
    public class ServerOptionsValidatorTests
    {
        [Fact]
        public void Validate_WithValidPort_ReturnsSuccess()
        {
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions { Port = 5000, MaxClients = 10 };
            var result = validator.Validate("Server", options);
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithValidMaxClients_ReturnsSuccess()
        {
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions { Port = 5000, MaxClients = 5 };
            var result = validator.Validate("Server", options);
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(65536)]
        public void Validate_WithInvalidPort_ReturnsFail(int invalidPort)
        {
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions { Port = invalidPort, MaxClients = 10 };
            var result = validator.Validate("Server", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(0)]
        public void Validate_WithInvalidMaxClients_ReturnsFail(int invalidMaxClients)
        {
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions { Port = 5000, MaxClients = invalidMaxClients };
            var result = validator.Validate("Server", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithBoundaryPortValue_ReturnsSuccess()
        {
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions { Port = 1024, MaxClients = 1 };
            var result = validator.Validate("Server", options);
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_WithHighMaxClients_ReturnsSuccess()
        {
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions { Port = 5000, MaxClients = 1000 };
            var result = validator.Validate("Server", options);
            Assert.Equal(ValidateOptionsResult.Success, result);
        }

        [Fact]
        public void Validate_ErrorMessageIsClear_OnInvalidPort()
        {
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions { Port = -1, MaxClients = 10 };
            var result = validator.Validate("Server", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
            var failures = result.Failures.ToList();
            Assert.NotEmpty(failures);
        }

        [Fact]
        public void Validate_ErrorMessageIsClear_OnInvalidMaxClients()
        {
            var validator = new ServerOptionsValidator();
            var options = new ServerOptions { Port = 5000, MaxClients = -1 };
            var result = validator.Validate("Server", options);
            Assert.NotEqual(ValidateOptionsResult.Success, result);
            Assert.NotNull(result.Failures);
        }
    }
}
