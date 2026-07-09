using LinkedInScheduler.API.DTOs;
using LinkedInScheduler.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkedInScheduler.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetAll()
    {
        var posts = await _postService.GetAllAsync();
        var response = posts.Select(p => new PostResponse(
            p.Id, p.Content, p.ScheduledAt, p.IsPublished, p.CreatedAt, p.PublishedAt));
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostResponse>> GetById(Guid id)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post is null) return NotFound();
        return Ok(new PostResponse(
            post.Id, post.Content, post.ScheduledAt, post.IsPublished, post.CreatedAt, post.PublishedAt));
    }

    [HttpPost]
    public async Task<ActionResult<PostResponse>> Create(CreatePostRequest request)
    {
        var post = await _postService.SchedulePostAsync(request.Content, request.ScheduledAt);
        var response = new PostResponse(
            post.Id, post.Content, post.ScheduledAt, post.IsPublished, post.CreatedAt, post.PublishedAt);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, response);
    }

    [HttpPatch("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        await _postService.PublishPostAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _postService.DeletePostAsync(id);
        return NoContent();
    }
}