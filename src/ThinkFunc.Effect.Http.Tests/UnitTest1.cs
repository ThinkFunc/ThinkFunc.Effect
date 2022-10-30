using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Xunit;
using System.Text.Json.Nodes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ThinkFunc.Effect.Http.Tests;


public record SampleResponse(string Hello);

public class HttpSpec
{
    [Fact]
    public async Task EchoTest()
    {
        using var sut = new WebApplicationFactory<Program>();

        using var client = sut.CreateClient();


        var ret = await client.PostAsJsonAsync("echo", new
        {
            Hello = "World"
        });

        var res = await JsonSerializer.DeserializeAsync<JsonNode>(ret.Content.ReadAsStream());

        var expect = JsonSerializer.SerializeToNode(new 
        {
            hello = "World"
        });

        Assert.True(res.DeepEquals(expect));

         //res.Should().BeEquivalentTo(expect);
    }
}
