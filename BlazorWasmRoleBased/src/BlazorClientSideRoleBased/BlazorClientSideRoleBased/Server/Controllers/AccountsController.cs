﻿using BlazorClientSideRoleBased.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlazorClientSideRoleBased.Server.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AccountsController : ControllerBase
  {
    private readonly UserManager<IdentityUser> _userManager;

    public AccountsController(UserManager<IdentityUser> userManager)
    {
      _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RegisterModel model)
    {
      var newUser = new IdentityUser { UserName = model.Email, Email = model.Email };

      var result = await _userManager.CreateAsync(newUser, model.Password);

      if (!result.Succeeded)
      {
        var errors = result.Errors.Select(x => x.Description);

        return BadRequest(new RegisterResult { Successful = false, Errors = errors });
      }

      // Add all new users to the User role
      await _userManager.AddToRoleAsync(newUser, "User");

      // Add new users whose email starts with 'admin' to the Admin role
      if (newUser.Email.StartsWith("admin"))
      {
        await _userManager.AddToRoleAsync(newUser, "Admin");
      }

      return Ok(new RegisterResult { Successful = true });
    }
  }
}
