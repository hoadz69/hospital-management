using Xunit;

namespace ApiGateway.Tests;

/// <summary>
/// Smoke test khẳng định test runner xUnit pickup được test case trong project này.
/// </summary>
/// <remarks>
/// Đây là cửa ngõ tối thiểu cho test infra Phase Pre-Phase 4 Hardening (P1.1).
/// Các test case nghiệp vụ thật cho API Gateway Application/Domain sẽ thêm
/// trong Phase 4 hoặc khi có yêu cầu coverage cụ thể.
/// </remarks>
public class SmokeTests
{
    /// <summary>
    /// Test runner phải pickup và pass test này để xác nhận xUnit đã nối đúng vào solution.
    /// </summary>
    [Fact]
    public void TestRunner_PicksUp_ApiGatewayTests()
    {
        Assert.Equal(2, 1 + 1);
    }
}
