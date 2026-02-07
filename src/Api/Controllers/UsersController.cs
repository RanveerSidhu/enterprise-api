using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/users")]

public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_userService.GetAllUsers());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        var createdUser = _userService.CreateUser(request);
        return CreatedAtAction(nameof(GetAll), new { id = createdUser.Id }, createdUser);
    }
    [HttpPut("{id}")]
    public IActionResult Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var updatedUser = _userService.UpdateUser(id, request);
        return Ok(updatedUser);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _userService.DeleteUser(id);
        return NoContent();
    }

}
