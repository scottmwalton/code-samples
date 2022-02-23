using BlazorClientSideAuth.Shared;

namespace BlazorClientSideAuth.Client.Services
{
  public interface IAuthService
  {
    Task<LoginResult> Login(LoginModel loginModel);
    Task Logout();
    Task<RegisterResult> Register(RegisterModel registerModel);
  }
}
