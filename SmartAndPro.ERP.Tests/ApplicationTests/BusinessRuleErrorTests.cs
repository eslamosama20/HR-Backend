using SmartAndPro.ERP.SharedKernel.BusinessRule;
using SmartAndPro.ERP.SharedKernel.Enums;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests
{
    public class BusinessRuleErrorTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            var ruleId = "Rule1";
            var errorMessage = "Error message 1";
            var errorCode = "E001";
            var severity = SeverityLevel.Critical;

            // Act
            var error = new BusinessRuleError(ruleId, errorMessage, errorCode, severity);

            // Assert
            Assert.Equal(ruleId, error.RuleId);
            Assert.Equal(errorMessage, error.ErrorMessage);
            Assert.Equal(errorCode, error.ErrorCode);
            Assert.Equal(severity, error.Severity);
        }
    }
}