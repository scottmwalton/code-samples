﻿using BlazorClientSideRoleBased.Shared;

namespace BlazorClientSideRoleBased.Client.Services
{
  public interface IAuthService
  {
    Task<LoginResult> Login(LoginModel loginModel);
    Task Logout();
    Task<RegisterResult> Register(RegisterModel registerModel);
  }
}