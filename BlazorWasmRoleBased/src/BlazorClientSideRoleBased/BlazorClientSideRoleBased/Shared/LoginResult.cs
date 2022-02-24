using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorClientSideRoleBased.Shared
{
  public class LoginResult
  {
    public bool Successful { get; set; }
    public string Error { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

  }
}
