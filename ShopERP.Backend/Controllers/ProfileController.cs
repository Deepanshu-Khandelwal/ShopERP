using Microsoft.AspNetCore.Mvc;
using ShopERP.Backend.Contracts.Requests;
using ShopERP.Backend.Contracts.Responses;
using ShopERP.Backend.Services;

namespace ShopERP.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(IProfileService profileService) : ControllerBase
{
    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetProfile(int userId, CancellationToken ct)
    {
        var profile = await profileService.GetProfileAsync(userId, ct);
        if (profile == null)
        {
            return NotFound("Profile not found");
        }

        return Ok(profile);
    }

    /// <summary>
    /// Get complete user data with profile and shop
    /// </summary>
    [HttpGet("user/{userId}/full")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserWithProfile(int userId, CancellationToken ct)
    {
        try
        {
            var user = await profileService.GetUserWithProfileAsync(userId, ct);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(int userId, [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        try
        {
            var profile = await profileService.UpdateProfileAsync(userId, request, ct);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
