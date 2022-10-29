using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ThinkFunc.Effect.Http.Tests;


public class HttpSpec
{
    [Fact]
    public async Task EchoTest()
    {
        using var sut = new WebApplicationFactory<Program>();

        using var client = sut.CreateClient();


        var ret = await client.PostAsJsonAsync("echo", new
        {
            Hello = "hello"
        });

        var res = await ret.Content.ReadFromJsonAsync<JsonDocument>();

        var expect = JsonSerializer.SerializeToDocument(new
        {
            Hello = "World"
        });

        res.Should().BeEquivalentTo(expect, opt => opt.ComparingByMembers<JsonElement>());
    }
}
