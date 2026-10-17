using System.Collections.Generic;
using SmartAndPro.ERP.SharedKernel.BusinessRule;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests
{
    public class BusinessRuleResultTests
    {
        [Fact]
        public void Success_CreatesSuccessfulResult()
        {
            // Act
            var result = BusinessRuleResult.Success();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Failure_CreatesFailureResult()
        {
            // Arrange
            var errors = new List<BusinessRuleError>
            {
                new BusinessRuleError("Rule1", "Error message 1")
            };

            // Act
            var result = BusinessRuleResult.Failure(errors);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(errors, result.Errors);
        }
    }
}