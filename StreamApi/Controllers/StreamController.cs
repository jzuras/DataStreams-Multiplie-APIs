using DataStreams.DataModel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Stream = DataStreams.DataModel.Models.Stream;

namespace DataStreams.StreamApi.Controllers;

[Route("/")]
[ApiController]
public class StreamController : ControllerBase
{
    private readonly StreamContext Context;

    public StreamController(StreamContext context)
    {
        Context = context;
    }

    // GET: /
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Stream>>> GetStreams()
    {
        var streams = await Context.Streams.ToListAsync();

        return Ok(streams);
    }

    // GET: /{name}
    [HttpGet("{name}")]
    public async Task<ActionResult<Stream>> GetStream(string name)
    {
        var stream = await Context.Streams.FindAsync(name);

        if (stream == null)
        {
            return NotFound();
        }

        return stream;
    }

    // PUT: /{name}
    [HttpPut("{name}")]
    public async Task<IActionResult> PutStream(string name, Stream stream)
    {
        if (name != stream.Name)
        {
            return BadRequest();
        }

        Context.Entry(stream).State = EntityState.Modified;

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StreamExists(name))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: /
    [HttpPost]
    public async Task<ActionResult<Stream>> PostStream(Stream stream)
    {
        Context.Streams.Add(stream);
        await Context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStream), new { name = stream.Name }, stream);
    }

    // DELETE: /{name}
    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteStream(string name)
    {
        var stream = await Context.Streams.FindAsync(name);
        if (stream == null)
        {
            return NotFound();
        }

        Context.Streams.Remove(stream);
        await Context.SaveChangesAsync();

        return NoContent();
    }

    private bool StreamExists(string name)
    {
        return Context.Streams.Any(e => e.Name == name);
    }
}
