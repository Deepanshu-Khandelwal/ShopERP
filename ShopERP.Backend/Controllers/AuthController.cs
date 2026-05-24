using Microsoft.AspNetCore.Mvc;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Contracts.Responses;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthenticationService authService) : ControllerBase
{
    /// <summary>
    /// Login user with username and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required");
        }

        var response = await authService.LoginAsync(request.Username, request.Password, ct);
        return Ok(response);
    }

    /// <summary>
    /// Create a new shop
    /// </summary>
    [HttpPost("shop/create")]
    [ProducesResponseType(typeof(ShopCreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShopCreateResponse>> CreateShop([FromBody] CreateShopRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Shop name and email are required");
        }

        var response = await authService.CreateShopAsync(request, ct);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return CreatedAtAction(nameof(CreateShop), response);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost("user/create")]
    [ProducesResponseType(typeof(UserCreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserCreateResponse>> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest("Username, email, password, and full name are required");
        }

        var response = await authService.CreateUserAsync(request, ct);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return CreatedAtAction(nameof(CreateUser), response);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(int userId, CancellationToken ct)
    {
        var user = await authService.GetUserAsync(userId, ct);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
    }
}
