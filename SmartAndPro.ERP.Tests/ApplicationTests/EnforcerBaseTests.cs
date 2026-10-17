using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartAndPro.ERP.Domains.BusinessRulesEnforcer;
using SmartAndPro.ERP.SharedKernel.BusinessRule;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests
{
    public class EnforcerBaseTests
    {
        private class TestEnforcer : EnforcerBase<string>
        {
            public override BusinessRuleResult EnforceRules(string entity) => BusinessRuleResult.Success();
            public override Task<BusinessRuleResult> EnforceRulesAsync(string entity) => Task.FromResult(BusinessRuleResult.Success());

            public BusinessRuleResult TestEnsureRules(IEnumerable<(Func<bool> rule, BusinessRuleError error)> rules)
            {
                return EnsureRules(rules);
            }

            public Task<BusinessRuleResult> TestEnsureRulesAsync(IEnumerable<(Func<Task<bool>> rule, BusinessRuleError error)> rules)
            {
                return EnsureRulesAsync(rules);
            }
        }

        [Fact]
        public void EnsureRules_AllRulesPass_ReturnsSuccess()
        {
            // Arrange
            var enforcer = new TestEnforcer();
            var rules = new List<(Func<bool> rule, BusinessRuleError error)>
            {
                (() => true, new BusinessRuleError("Rule1", "Error message 1")),
                (() => true, new BusinessRuleError("Rule2", "Error message 2"))
            };

            // Act
            var result = enforcer.TestEnsureRules(rules);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void EnsureRules_AnyRuleFails_ReturnsFailure()
        {
            // Arrange
            var enforcer = new TestEnforcer();
            var rules = new List<(Func<bool> rule, BusinessRuleError error)>
            {
                (() => true, new BusinessRuleError("Rule1", "Error message 1")),
                (() => false, new BusinessRuleError("Rule2", "Error message 2"))
            };

            // Act
            var result = enforcer.TestEnsureRules(rules);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal("Rule2", result.Errors[0].RuleId);
        }

        [Fact]
        public async Task EnsureRulesAsync_AllRulesPass_ReturnsSuccess()
        {
            // Arrange
            var enforcer = new TestEnforcer();
            var rules = new List<(Func<Task<bool>> rule, BusinessRuleError error)>
            {
                (async () => await Task.FromResult(true), new BusinessRuleError("Rule1", "Error message 1")),
                (async () => await Task.FromResult(true), new BusinessRuleError("Rule2", "Error message 2"))
            };

            // Act
            var result = await enforcer.TestEnsureRulesAsync(rules);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task EnsureRulesAsync_AnyRuleFails_ReturnsFailure()
        {
            // Arrange
            var enforcer = new TestEnforcer();
            var rules = new List<(Func<Task<bool>> rule, BusinessRuleError error)>
            {
                (async () => await Task.FromResult(true), new BusinessRuleError("Rule1", "Error message 1")),
                (async () => await Task.FromResult(false), new BusinessRuleError("Rule2", "Error message 2"))
            };

            // Act
            var result = await enforcer.TestEnsureRulesAsync(rules);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal("Rule2", result.Errors[0].RuleId);
        }
    }
}
