using Innout.Errors;
using Innout.Services.Redis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Innout.Controllers;

[ApiController]
[Route("[Controller]")]
public class TrailController(IRedisCacheService redisCacheService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await redisCacheService.GetAsync<Todo>("one");

        if (result is not null)
        {
            return Ok(new { fromCache = true, todo = result });
        }

        var todo = new Todo(1, "Play a game");

        await redisCacheService.SetAsync<Todo>("one", todo, TimeSpan.FromMinutes(1));

        return Ok(new { todo });
    }

    private record Todo(int id, string name);
}

