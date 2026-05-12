using ClinicSaaS.BuildingBlocks.Tenancy;
using ClinicSaaS.Contracts.Templates;
using TemplateService.Application.Templates;
using Xunit;

namespace TemplateService.Tests;

/// <summary>
/// Unit test nhỏ cho contract/stub handler của Template Service.
/// </summary>
public sealed class TemplateContractStubHandlerTests
{
    /// <summary>
    /// Xác nhận registry dummy có ít nhất một template để FE Wave A ghép card.
    /// </summary>
    [Fact]
    public void ListTemplates_ReturnsRegistryItems()
    {
        var handler = new TemplateContractStubHandler(new TenantContextAccessor());

        var result = handler.ListTemplates();

        Assert.NotEmpty(result.Items);
    }

    /// <summary>
    /// Xác nhận mode apply không hợp lệ trả lỗi validation rõ.
    /// </summary>
    [Fact]
    public void ApplyTemplate_InvalidMode_ReturnsValidation()
    {
        var tenantId = Guid.NewGuid();
        var accessor = new TenantContextAccessor();
        accessor.SetCurrent(new TenantContext(tenantId.ToString(), "test"));
        var handler = new TemplateContractStubHandler(accessor);

        var result = handler.ApplyTemplate(tenantId, new ApplyTemplateRequest("dental", "bad-mode", "tester"));

        Assert.False(result.IsSuccess);
        Assert.Equal("templates.validation", result.Error.Code);
    }
}
