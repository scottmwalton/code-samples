using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorClientSideRoleBased.Shared
{
  public class RegisterResult
  {
    public bool Successful { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
  }
}
