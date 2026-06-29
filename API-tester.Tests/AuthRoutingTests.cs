using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace API_tester.Tests;

public class AuthRoutingTests
{
    [Fact]
    public async Task Login_IsPublicAndDoesNotRenderAuthenticatedNavigation()
    {
        using var factory = Lab5ApiFactory.CreateWithApplicationAuthentication();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("class=\"app-header\"", html);
        Assert.DoesNotContain(">Dashboard</a>", html);
    }

    [Theory]
    [InlineData("/workspaces")]
    [InlineData("/collections")]
    [InlineData("/requests")]
    [InlineData("/request-builder")]
    [InlineData("/environments")]
    [InlineData("/tags")]
    public async Task ProtectedPages_RedirectAnonymousUsersToLogin(string path)
    {
        using var factory = Lab5ApiFactory.CreateWithApplicationAuthentication();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Root_RedirectsAnonymousUsersToLogin()
    {
        using var factory = Lab5ApiFactory.CreateWithApplicationAuthentication();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Dashboard_RendersAuthenticatedNavigationForAuthenticatedUser()
    {
        using var factory = Lab5ApiFactory.CreateWithRoles("Admin");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/dashboard");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("class=\"app-header\"", html);
        Assert.Contains(">Dashboard</a>", html);
    }
}
