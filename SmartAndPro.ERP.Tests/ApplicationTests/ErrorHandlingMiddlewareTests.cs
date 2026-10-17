using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SmartAndPro.ERP.SharedKernel.BusinessRule;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Exceptions;
using SmartAndPro.ERP.WebAPI.Middlewares;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests
{
    public class ErrorHandlingMiddlewareTests
    {
        private TestServer CreateTestServer()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddTransient<ErrorHandlingMiddleware>();
                    services.AddLogging();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<ErrorHandlingMiddleware>();
                    app.Run(async context =>
                    {
                        var errors = new List<BusinessRuleError>
                        {
                            new BusinessRuleError("Rule1", "Error message 1", "E001", SeverityLevel.Error),
                            new BusinessRuleError("Rule2", "Error message 2", "E002", SeverityLevel.Critical)
                        };
                        throw new BusinessRuleViolationException(errors);
                    });
                });

            return new TestServer(builder);
        }

        [Fact]
        public async Task InvokeAsync_BusinessRuleViolationException_ReturnsBadRequest()
        {
            // Arrange
            var server = CreateTestServer();
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);
            errorResponse.Message.Should().Be("One or more business rules were violated.");
            errorResponse.Errors.Should().HaveCount(2);
            errorResponse.Errors[0].RuleId.Should().Be("Rule1");
            errorResponse.Errors[1].RuleId.Should().Be("Rule2");
        }

        [Fact]
        public async Task InvokeAsync_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddTransient<ErrorHandlingMiddleware>();
                    services.AddLogging();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<ErrorHandlingMiddleware>();
                    app.Run(context =>
                    {
                        throw new Exception("Unexpected error");
                    });
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var responseString = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);
            errorResponse.Message.Should().Be("An unexpected error occurred.");
            errorResponse.Errors.Should().BeEmpty();
        }
    }
}
