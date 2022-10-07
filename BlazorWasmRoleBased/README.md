# Configuring Role Based Authorization with client-side Blazor

*Full disclosure - This sample is completely copied from the blog post by Chris Sainty titled ["Configuring Role Based Athorization with client-side Blazor"](https://chrissainty.com/securing-your-blazor-apps-configuring-role-based-authorization-with-client-side-blazor/).  That post was configured for .NET Core 3.1.  I have updated it for .NET 6*

## What is role-based authorization?

When it comes to authorization in ASP.NET Core we have two options, role-based and policy-based (there’s also claims-based but thats just a special type of policy-based).

Role-based authorization has been around for a while now and was originally introduced in ASP.NET (pre-Core). It’s a declarative way to restrict access to resources.

Developers can specify the name of the particular role a user must be a member of in order to access a certain resource. This is most commonly done using the ```[Authorize]``` attribute by specifying a role or list of roles - ```[Authorize(Roles = “Admin”)]```. Users can be a member of a single role or multiple roles.

How roles are created and managed is dependent on the backing store used. As we’ve been using ASP.NET Core Identity in the series so far we’ll continue use it to manage and store our roles.

We’ll be building on top of the application we build in [this sample](https://github.com/scottmwalton/code-samples/tree/master/BlazorWasmDemo).

### Setting up Roles with ASP.NET Core Identity

We need to add the role specific services to our application. To do this, we need to update the code in the ```Program.cs``` file.

Replace this

```c#
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

```

With this

```c#
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
```

The ```IdentityRole``` type is the default role type provided by ASP.NET Core Identity. But you can provide a different type if it doesn’t fit your requirements.

Next, we’re going to seed our database with some roles - we’re going to add a *User* and *Admin* role. To do this we’re going to override the ```OnModelCreating``` method of the ```ApplicationDbContext```.

```c#
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(new IdentityRole { Name = "User", NormalizedName = "USER", Id = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString() });
        builder.Entity<IdentityRole>().HasData(new IdentityRole { Name = "Admin", NormalizedName = "ADMIN", Id = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString() });
    }
}
```

Once this is done we need to generate a migration and then apply it to the database.

```Powershell
Add-Migration SeedRoles -o Data/Migrations
Update-Database
```

### Adding users to roles

Now we have some roles available, we’re going to update the action on the Accounts controller which creates new users.

We’re going to add all new users to the *User* role. Except if the new users email starts with admin. If it does, then we’re going to add them to *User* and *Admin* groups. Update the ```AccountsController``` to look like the code below.

```c#
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
```

We’re now assigning users to roles at signup but we need to pass this information down to Blazor. To do this, we need to update the claims we are putting into our JSON Web Token.

### Adding roles as claims to the JWT

In the Login controller we’re going to update the ```Login``` method. Let’s remove the current line generating claims.

```c#
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, login.Email)
		};
```

And replace it with the following.

```c#
     var user = await _signInManager.UserManager.FindByEmailAsync(login.Email);
     var roles = await _signInManager.UserManager.GetRolesAsync(user);

     var claims = new List<Claim>();

     claims.Add(new Claim(ClaimTypes.Name, login.Email));

     foreach (var role in roles)
     {
       claims.Add(new Claim(ClaimTypes.Role, role));
     }
```

We start off by getting the current user via the ```UserManager```, which we then use to get their roles. The original ```Name``` claim is added with the users email, as before. If any roles are present we loop over them and each one is added as a ```Role``` claim.

It’s important to understand a quirk about role claims at this point. You may expect that if a user is in two roles then two role claims will be added to the JWT.

```ruby
http://schemas.microsoft.com/ws/2008/06/identity/claims/role - "User"
http://schemas.microsoft.com/ws/2008/06/identity/claims/role - "Admin"
```

But that’s not what happens, what happens is that the two role claims get combined into an array.

```ruby
http://schemas.microsoft.com/ws/2008/06/identity/claims/role - ["User", "Admin"]
```

This is important because on the client we are going to have to workout if we’re dealing with an array or a single value. If we’re dealing with an array then we will need to do some extra work to get the individual roles out.

### Working with roles in client-side Blazor

We’re looking pretty good so far. We have new users being added to roles and once they have signed in we are returning those roles via the JWT. But how can we use roles inside of Blazor?

At this point in time there isn’t anything official to help us with roles, so we’ve got to deal with it manually.

In part 2 of the series we added the ```ApiAuthenticationStateProvider``` class, which has a method called ```ParseClaimsFromJwt``` that looks like this.

```c#
	private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
	{
		var claims = new List<Claim>();
		var payload = jwt.Split('.')[1];
		var jsonBytes = ParseBase64WithoutPadding(payload);
		var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
		if (keyValuePairs != null)
		{
			keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles);
			if (roles != null)
			{
				if ((roles.ToString() ?? string.Empty).Trim().StartsWith("["))
				{
					var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString() ?? string.Empty);
					if (parsedRoles != null)
					{
						foreach (var parsedRole in parsedRoles)
						{
							claims.Add(new Claim(ClaimTypes.Role, parsedRole));
						}
					}
				}
				else
				{
					claims.Add(new Claim(ClaimTypes.Role, roles.ToString() ?? string.Empty));
				}
				keyValuePairs.Remove(ClaimTypes.Role);
			}
			claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty)));
		}
		return claims;
	}

	private byte[] ParseBase64WithoutPadding(string base64)
	{
		switch (base64.Length % 4)
		{
			case 2: base64 += "=="; break;
			case 3: base64 += "="; break;
		}
		return Convert.FromBase64String(base64);
	}
```

As we saw in part 2 it takes a JWT, decodes it, extracts the claims and returns them. But what we didn't cover was that I modified it to handle roles as a special case.

If a role claim is present then we check if the first character is a ```[``` indicating it’s a JSON array. If the character is found then ```roles``` is parsed again to extract the individual role names. We then loop through the role names and add each as a claim. If ```roles``` is not an array then its added as a single role claim.

I admit this is not the prettiest code and I’m sure it could be made much better but it serves our purpose for now.

We need to update the ```MarkUserAsAuthenticated``` method to call ```ParseClaimsFromJwt```.

```c#
   public void MarkUserAsAuthenticated(string token)
   {
     var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
     var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
     NotifyAuthenticationStateChanged(authState);
   }
```

Finally, we need to update the ```Login``` method on the ```AuthService``` to pass the token rather than the email when calling ```MarkUserAsAuthenticated```.

```c#
   public async Task<LoginResult> Login(LoginModel loginModel)
   {
     var loginAsJson = JsonSerializer.Serialize(loginModel);
     var response = await _httpClient.PostAsync("api/Login", new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
     var loginResult = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

     if (!response.IsSuccessStatusCode)
     {
       return loginResult;
     }

     await _localStorage.SetItemAsync("authToken", loginResult.Token);
     ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);
     _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

     return loginResult;
   }
```

We should now have the ability to apply role based authorization to our app. Let’s start at the API.

### Applying role-based authorization to the API

Let’s set the ```Get``` action on the ```WeatherForecastController``` to only be accessible to authenticated users in the Admin role. We do this by using the ```Authorize``` attribute and specifying the roles that are allowed to access it.

```csharp
	[Authorize(Roles = "Admin")]
	[HttpGet]
	public IEnumerable<WeatherForecast> Get()
	{
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast
		{
			Date = DateTime.Now.AddDays(index),
			TemperatureC = Random.Shared.Next(-20, 55),
			Summary = Summaries[Random.Shared.Next(Summaries.Length)]
		})
		.ToArray();
	}
```

If you create a new user in the *Admin* role and go to the Fetch Data page in the Blazor app you should still see everything load as expected.

![Authorized User View](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmRoleBased/assets/images/WeatherForecastAuthorized.png "Authorized User View")

But if you create a normal user and do the same, you should see the page stuck with a Loading… message and an Unhandled Excpetion at the bottom. The unhandled exception is due to the API receiving an unauthorized request and redirecting to an error page with HTML. The Blazor page is expecting a JSON result instead of HTML.

![Unauthorized User View](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmRoleBased/assets/images/WeatherForecastUnauthorized.png "Unauthorized User View")

Just for reference, as well as applying the ```Authorize``` attribute to actions you can also apply to it a controller. When applied at a controller level all actions on that controller are protected.

### Applying role-based authorization in Blazor

Blazor can also use the ```Authorize``` attribute to protect pages. This is achieved by using the ```@attribute``` directive to apply the ```[Authorize]``` attribute. You can also restrict access to parts of a page using the ```AuthorizeView``` component.

> ***Warning*** *- Any client-side checks can be bypassed as the user can potentially modify any of the code. This is true for any client-side technology, so make sure you always have checks on your API as well.*

As the forecast data is only available to Admin users let’s restrict access to that page using the ```Authorize``` attribute.

```html
@page "/fetchdata"
@attribute [Authorize(Roles = "Admin")]
```

You will also need to add another using statement to our ```_Imports.razor``` page.

```html
@using Microsoft.AspNetCore.Authorization
```

Now try logging into that page using your admin user. Everything should continue to work. Then try logging in as the standard user, you should now see a *Not authorized* message.

![Unauthorized User View 2](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmRoleBased/assets/images/WeatherForecastUnauthorized2.png "Unauthorized User View 2")

Let’s test out the ```AuthorizeView``` as well. On the home page (index.razor) add the following code.

```html
<AuthorizeView Roles="User">
    <p>You can only see this if you're in the User role.</p>
</AuthorizeView>

<AuthorizeView Roles="Admin">
    <p>You can only see this if you're in the Admin role.</p>
</AuthorizeView>
```

Again, log in with your admin and user accounts. When you’re logged in as the admin user you should see both messages, as you’re in both roles.

![Index Amdin Role](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmRoleBased/assets/images/IndexAdminRole.png "Index Admin Role")

When you’re logged in as a standard user you should only see the first message.

![Index User Role](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmRoleBased/assets/images/IndexUserRole.png "Index User Role")

Summary
In this sample, we’ve looked at what role-based authorization is and how to use ASP.NET Core Identity to setup and mange roles. We then moved on to how to pass roles as claims using JSON Web Tokens from the API to the client. Then we worked through processing those role claims in Blazor and finally implemented some roles based authorization checks on both the API and Blazor.

I just want to reiterate that you cannot just rely on client-side authentication or authorization, the client can never be trusted. You must always perform authentication and authorization checks on the server as well.
