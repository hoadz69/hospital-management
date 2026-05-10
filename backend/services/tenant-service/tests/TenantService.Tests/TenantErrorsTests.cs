using TenantService.Application.Tenants;
using Xunit;

namespace TenantService.Tests;

/// <summary>
/// Unit test xác nhận factory <see cref="TenantErrors"/> sinh đúng <c>Error</c> để API layer
/// build ProblemDetails 409 chứa <c>extensions.fields</c> cho FE định vị input bị conflict.
/// </summary>
public sealed class TenantErrorsTests
{
    /// <summary>
    /// Khi gọi <see cref="TenantErrors.Conflict(string, System.Collections.Generic.IReadOnlyList{string})"/>
    /// với 1 field, Error trả về phải có Code = `tenants.conflict` và Details["fields"] = [field code]
    /// để API layer pass qua `extensions.fields`.
    /// </summary>
    [Fact]
    public void Conflict_WithSlugField_PutsFieldIntoDetailsKey()
    {
        var error = TenantErrors.Conflict(
            "Tenant slug or domain already exists.",
            new[] { TenantErrors.FieldSlug });

        Assert.Equal("tenants.conflict", error.Code);
        Assert.NotNull(error.Details);
        Assert.True(error.Details!.ContainsKey(TenantErrors.FieldsDetailKey));
        Assert.Equal(new[] { "slug" }, error.Details[TenantErrors.FieldsDetailKey]);
    }

    /// <summary>
    /// Khi gọi <see cref="TenantErrors.Conflict(string, System.Collections.Generic.IReadOnlyList{string})"/>
    /// với nhiều field, Details["fields"] phải giữ nguyên thứ tự caller truyền vào để FE highlight
    /// đúng input field bị conflict.
    /// </summary>
    [Fact]
    public void Conflict_WithMultipleFields_PreservesOrder()
    {
        var error = TenantErrors.Conflict(
            "Tenant slug or domain already exists.",
            new[] { TenantErrors.FieldSlug, TenantErrors.FieldDefaultDomainName });

        Assert.NotNull(error.Details);
        Assert.Equal(
            new[] { "slug", "defaultDomainName" },
            error.Details![TenantErrors.FieldsDetailKey]);
    }

    /// <summary>
    /// Overload không nhận fields phải giữ backward-compat: không sinh Details để API layer
    /// fallback về ProblemDetails 409 không có `extensions.fields` như trước Phase 2.
    /// </summary>
    [Fact]
    public void Conflict_WithoutFields_ReturnsErrorWithoutDetails()
    {
        var error = TenantErrors.Conflict("Tenant slug or domain already exists.");

        Assert.Equal("tenants.conflict", error.Code);
        Assert.Null(error.Details);
    }
}
