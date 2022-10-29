using Microsoft.AspNetCore.Mvc;

namespace ThinkFunc.Effect.Http.StubApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EchoController : ControllerBase
    {
        [HttpPost()]
        public async Task Post()
        {
            await Results.Ok("""
                { "Hello" : "World" }
            """).ExecuteAsync(HttpContext);
        }
    }
}