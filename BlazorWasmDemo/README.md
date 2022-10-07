# Authentication with Client-Side Blazor using WebAPI and ASP.NET Core Identity

*Full disclosure - This sample is completely copied from the blog post by Chris Sainty titled ["Athentication with client-side Blazor using WebAPI and APS.NET Core Identity"](https://chrissainty.com/securing-your-blazor-apps-authentication-with-clientside-blazor-using-webapi-aspnet-core-identity/).  That post was configured for .NET Core 3.1.  I have updated it for .NET 6*

## Getting Setup: Creating the solution

![Create Blazor Solution Step 1](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmDemo/assets/images/create_blazor_solution1.png "Create Blazor Solution Step 1")

Start by creating a new Blazor WebAssembly App, this template will create a Blazor application which runs in the 
clients browser on WebAssembly hosted by a ASP.NET Core WebAPI. Search for "Blazor" templates if it does not appear in the list.

![Create Blazor Solution Step 2](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmDemo/assets/images/create_blazor_solution2.png "Create Blazor Solution Step 2")

Fill in the appropriate values for Project Name, Location and Solution Name

![Create Blazor Solution Step 3](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmDemo/assets/images/create_blazor_solution3.png "Create Blazor Solution Step 3")

On the Additional information window, remember to tick the ASP.NET Core hosted checkbox. Eventhough we are creating an app
with Authentication, make sure to select "None" for the Authentication type. Once the solution has been created we're going 
to start making some changes to the server project.

## Configuring WebAPI

We're goint to configure the API first, but before we begin let's get some NuGet packages installed.

```xml
	<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.2" />
	<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="6.0.2" />
	<PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.2" />
	<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.2">
		<PrivateAssets>all</PrivateAssets>
		<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
	</PackageReference>
	<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.2" />
	<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.2">
		<PrivateAssets>all</PrivateAssets>
		<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
	</PackageReference>
```

You can either add the above packages to your server projects .csproj file - or you can install them via the command line or NuGet package manager.

### Setting up the Identity database: Connection string

Before we can set anything up, database wise we need a connection string. This is usually kept in the ```appsettings.json``` file.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BlazorClientSideAuth;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

The file comes with a couple of settings already in place. Add the "ConnectionStrings" setting to the end of the file. You will need to update the "DefaultConnection" value to the appropriate values for you.

### Setting up the Identity database: DbContext

In the root of the server project crate a folder called ```Data``` then add a new class called ```ApplicationDbContext``` with the follwing code.

```c#
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
}
```

Because we are using Identity which needs to store information in a database we're not inheriting from ```DbContext``` but instead from ```IdentityDbContext```.  The ```IdentityDbContext``` base class contains all the configuration EF needs to manage the Identity database tables.

### Setting up the Identity database: Registering services

In the ```Program.cs``` file we need to add code to load our configuration from ```appsettings.json``` add the following code to the top of the file.

```c#
// Setup Configuration
var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json")
			.Build();
```

This tells the program to use the ```appsettings.json``` file and load it as an ```IConfiguration``` object. This will allow us to use our Connection Strings we defined earlier. 

```c#
// code removed for brevity

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

// code removed for brevity
```

Now add the two services after the line ```builder.Services.AddRazorPages();```. Essentially, these two lines are adding the ```ApplicationDbContext``` to the services collection. Then registering the various services for ASP.NET Core Identity and telling it to use Entity Framework as a backing store via the ```ApplicationDbContext```.

### Setting up the Identity database: Creating the database
We're now in a position to create the initial migration for the database. In the package manager console run the following command.

```Powershell
Add-Migration CreateIdentitySchema -o Data/Migrations
```

Once the command has run you should see the migrations file in ```Data``` > ```Migrations```. Run ```Update-Database``` in the console to apply the migration to your database.

If you have any issues with running the migration command, make sure that the server project is selected as the default project in the package manager console.

### Enabling Authentication: Registering services

The next step is to enable authentication in the API. Again, in ```Program.cs``` file add the following code after the code we added in the previous section.

```c#
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = configuration["JwtIssuer"],
				ValidAudience = configuration["JwtAudience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]))
			};
		});
```

The code above is adding and setting up some services required for authentication to the service container. Then adding a handler for JSON Web Tokens (JWT) and configuring how received JWTs should be validated. Feel free to tweak these settings to your requirements.

### Enabling Authentication: App settings

There are a few settings which are being loaded from the ```appsettings.json``` file.

- ```Configuration["JwtIssuer"]```
- ```Configuration["JwtAudience"]```
- ```Configuration["JwtSecurityKey"]```

We haven't actually added them to the ```appsettings.json``` file yet, so let do that now. While we're there we'll also add a setting to control how long the tokens last, which we'll use in a bit.

```json
"JwtSecurityKey": "RANDOM_KEY_MUST_NOT_BE_SHARED",
"JwtIssuer": "https://localhost",
"JwtAudience": "https://localhost",
"JwtExpiryInDays": 1,
```

It's really important that the ```JwtSecurityKey``` is kept secret as this is what is used to sign the tokens produced by the API. If this is compromised then your app would no longer be secure.

As I'm running everything locally I have my *Issuer* and *Audience* set to localhost. But if you're using this in a real app then you would set the *Issuer* to the domain the API is running on and the *Audience* to the domain the client app is running on.

### Enabling Authentication: Adding middleware

Finally, in the ```Program.cs``` file we need to add the necessary middleware to the pipeline. This will enable the authentication and authorization features in our API. Add them just after the ```app.UseRouting();``` middleware.

```c#
app.UseAuthentication();
app.UseAuthorization();
```

That should be everything we need to do the ```Program.cs``` file. Authentication is now enabled for the API.

You can test everything is working by adding an ```[Authorize]``` attribute to the ```WeatherForecasts``` action on the ```WeatherForecastController```. Then startup the app and navigate to the Fetch Data page, no data should load and you should see a 401 error in the console.

### Adding the account controller

In order for people to login to our app they need to be able to signup. We're going to add an account controller which will be responsible for creating new accounts.

```c#
[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public AccountsController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]RegisterModel model)
    {
        var newUser = new IdentityUser { UserName = model.Email, Email = model.Email };

        var result = await _userManager.CreateAsync(newUser, model.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description);

            return Ok(new RegisterResult { Successful = false, Errors = errors });

        }

        return Ok(new RegisterResult { Successful = true });
    }
}
```

The ```Post``` action uses the ASP.NET Core Identity ```UserManager``` to create a new user in the system from a ```RegisterModel```.

We haven't added the register model yet so we can do that now, put this in the shared project as this will be used by our Blazor app in a bit.

```c#
public class RegisterModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

If all goes well then a successful ```RegisterResult``` is returned, otherwise a failed ```RegisterResult``` is returned. Again, we need to create the ```RegisterResult``` and again it needs to go in the shared project.

```c#
public class RegisterResult
{
    public bool Successful { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
```

### Adding the login controller

Now we have a way for users to signup we now need a way for them to login.

```c#
[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SignInManager<IdentityUser> _signInManager;

    public LoginController(IConfiguration configuration,
                           SignInManager<IdentityUser> signInManager)
    {
        _configuration = configuration;
        _signInManager = signInManager;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, false, false);

        if (!result.Succeeded) return BadRequest(new LoginResult { Successful = false, Error = "Username and password are invalid." });

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

        var token = new JwtSecurityToken(
            _configuration["JwtIssuer"],
            _configuration["JwtAudience"],
            claims,
            expires: expiry,
            signingCredentials: creds
        );

        return Ok(new LoginResult { Successful = true, Token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
```

The sole job of the login controller is to verify the username and password in the ```LoginModel``` using the ASP.NET Core Identity ```SignInManger```. If they're correct then a new JSON web token is generated and passed back to the client in a ```LoginResult```.

Just like before we need to add the ```LoginModel``` and ```LoginResult``` to the shared project.

```c#
  public class LoginModel
  {
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
  }
```

```c#
  public class LoginResult
  {
    public bool Successful { get; set; }
    public string Error { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
  }
```

That's everything we need on our API. We have now configured it to use authentication via JSON web tokens. As well as setup the controllers we need for our Blazor client-side app to register new users and to login.


## Configuring client-side Blazor

Let's turn our attention to Blazor. The first thing we're going to do is install [Blazored.LocalStorage](https://www.nuget.org/packages/Blazored.LocalStorage/), we will need this later to persist the auth token from the API when we login.

Before adding the package to your project, make sure your Package Manager Console is pointing to the Client project!

```Powershell
Install-Package Blazored.LocalStorage
```

We also need to update the ```App.razor``` component to use the ```AuthorizeRouteView``` component instead of the ```RouteView``` component.

```html
<Router AppAssembly="@typeof(App).Assembly">
	<Found Context="routeData">
		<AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />	
	</Found>
	<NotFound>
		<CascadingAuthenticationState>
			<LayoutView Layout="@typeof(MainLayout)">
				<p>Sorry, there's nothing at this address.</p>
			</LayoutView>
		</CascadingAuthenticationState>
	</NotFound>
</Router>
```

This component provides a cascading parameter of type ```Task<AuthenticationState>```. This is used by the ```AuthorizeView``` component to determine the current users authentication state.

But any component can request the parameter and use it to do procedural logic, for example.

```html
@code {
    [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }

    private async Task LogUserAuthenticationState()
    {
        var authState = await authenticationStateTask;
        var user = authState.User;

        if (user.Identity.IsAuthenticated)
        {
            Console.WriteLine($"User {user.Identity.Name} is authenticated.");
        }
        else
        {
            Console.WriteLine("User is NOT authenticated.");
        }
    }
}
```

### Creating a Custom AuthenticationStateProvider

As we are using client-side Blazor we need to provide our own implementation for the ```AuthenticationStateProvider``` class. Because there are so many options when it comes to client-side apps there is no way to design a default class that would work for everyone.

We need to override the ```GetAuthenticationStateAsync``` method. In this method we need to determine if the current user is authenticated or not. We're also going to add a couple of helper methods which we will use to update the authentication state when the user logs in or out.

```c#
public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly HttpClient _httpClient;
	private readonly ILocalStorageService _localStorage;
	public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
	{
		_httpClient = httpClient;
		_localStorage = localStorage;
	}
		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var savedToken = await _localStorage.GetItemAsync<string>("authToken");
			if (string.IsNullOrWhiteSpace(savedToken))
		{
			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
		}
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);
			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));
	}
		public void MarkUserAsAuthenticated(string token)
	{
		var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
		var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
		NotifyAuthenticationStateChanged(authState);
	}
		public void MarkUserAsLoggedOut()
	{
		var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
		var authState = Task.FromResult(new AuthenticationState(anonymousUser));
		NotifyAuthenticationStateChanged(authState);
	}
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
}
```

There is a lot of code here so let's break it down step by step.

The ```GetAuthenticationStateAsync``` method is called by the ```CascadingAuthenticationState``` component to determine if the current user is authenticated or not.

In the code above, we check to see if there is an auth token in local storage. If there is no token in local storage, then we return a new ```AuthenticationState``` with a blank claims principal. This is the equivalent of saying the current user is not authenticated.

If there is a token, we retrieve it and set the default authorization header for the ```HttpClient```. We then return a new ```AuthenticationState``` with a new claims principal containing the claims from the token. The claims are extracted from the token by the ```ParseClaimsFromJwt``` method. This method decodes the token and returns the claims contained within it.

*Full disclosure - the ParseClaimsFromJwt method is borrowed from [Steve Sandersons Mission Control](https://github.com/SteveSandersonMS/presentation-2019-06-NDCOslo/tree/master/demos/MissionControl) demo app, which he showed at [NDC Oslo 2019](https://www.youtube.com/watch?v=uW-Kk7Qpv5U).*

The ```MarkUserAsAuthenticated``` is a helper method that's used to when a user logs in. Its sole purpose is to invoke the ```NotifyAuthenticationStateChanged``` method which fires an event called ```AuthenticationStateChanged```. This cascades the new authentication state, via the ```CascadingAuthenticationState``` component.

As you may expect, ```MarkUserAsLoggedOut``` does almost exactly the same as the previous method but when a user logs out.

### Auth Service

The auth service is going to be the what we use in our components to register users and log them in and out of the application. It's going to be a nice abstraction for all of the stuff going on in the background.  We will need to create both the ```IAuthService``` interface and the ```AuthService``` class. Create a new folder in the root of the Client project called ```Services``` and add the interface and class below.

```c#
  public interface IAuthService
  {
    Task<LoginResult> Login(LoginModel loginModel);
    Task Logout();
    Task<RegisterResult> Register(RegisterModel registerModel);
  }
```

```c#
  public class AuthService : IAuthService
  {
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient,
                       AuthenticationStateProvider authenticationStateProvider,
                       ILocalStorageService localStorage)
    {
      _httpClient = httpClient;
      _authenticationStateProvider = authenticationStateProvider;
      _localStorage = localStorage;
    }

    public async Task<RegisterResult> Register(RegisterModel registerModel)
    {
      //var result = await _httpClient.PostAsJsonAsync<RegisterResult>("api/accounts", registerModel);

      var registerAsJson = JsonSerializer.Serialize(registerModel);
      var response = await _httpClient.PostAsync("api/Accounts", new StringContent(registerAsJson, Encoding.UTF8, "application/json"));
      var registerResult = JsonSerializer.Deserialize<RegisterResult>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
      return registerResult;
    }

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
      //((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginModel.Email);
      ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

      return loginResult;
    }

    public async Task Logout()
    {
      await _localStorage.RemoveItemAsync("authToken");
      ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
      _httpClient.DefaultRequestHeaders.Authorization = null;
    }
  }
```

The ```Register``` method posts the ```RegisterModel``` to the accounts controller and then returns the ```RegisterResult``` to the caller.

The ```Login``` method is similar to the ```Register``` method, it posts the ```LoginModel``` to the login controller. But when a successful result is returned it strips out the auth token and persists it to local storage.

It then calls the ```MarkUserAsAuthenticated``` method we just looked at on the ```ApiAuthenticationStateProvider```. Finally, it sets the default authorization header on the ```HttpClient```.

The ```Logout``` method is just doing the reverse of the ```Login``` method.

### Razor Imports

We need to add a few using statments into the ```_Imports.razor``` file. Add the following lines to the end of the file. You will need to update the first two lines with the appropriate namespace for your project.

```html
@using BlazorClientSideAuth.Client.Services
@using BlazorClientSideAuth.Shared
@using Microsoft.AspNetCore.Components.Authorization
```

### Register Component

We're on the home stretch now. We can now turn our attention to the UI and creating a component which will allow people to register with the site.

```html
@page "/register"
@inject IAuthService AuthService
@inject NavigationManager NavigationManager

<h1>Register</h1>

@if (ShowErrors)
{
	<div class="alert alert-danger" role="alert">
		@foreach (var error in Errors)
		{
			<p>@error</p>
		}
	</div>
}

<div class="card">
	<div class="card-body">
		<h5 class="card-title">Please enter your details</h5>
		<EditForm Model="registerModel" OnValidSubmit="HandleRegistration">
			<DataAnnotationsValidator />
			<ValidationSummary />

			<div class="form-group">
				<label for="email">Email address</label>
				<InputText Id="email" class="form-control" @bind-Value="registerModel.Email" />
				<ValidationMessage For="@(() => registerModel.Email)" />
			</div>
			<div class="form-group">
				<label for="password">Password</label>
				<InputText Id="password" type="password" class="form-control" @bind-Value="registerModel.Password" />
				<ValidationMessage For="@(() => registerModel.Password)" />
			</div>
			<div class="form-group">
				<label for="password">Confirm Password</label>
				<InputText Id="password" type="password" class="form-control" @bind-Value="registerModel.ConfirmPassword" />
				<ValidationMessage For="@(() => registerModel.ConfirmPassword)" />
			</div>
			<button type="submit" class="btn btn-primary">Submit</button>
		</EditForm>
	</div>
</div>

@code {

	private RegisterModel registerModel = new RegisterModel();
	private bool ShowErrors;
	private IEnumerable<string> Errors = Enumerable.Empty<string>();

	private async Task HandleRegistration()
	{
		ShowErrors = false;

			var result = await AuthService.Register(registerModel);

			if (result.Successful)
			{
				NavigationManager.NavigateTo("/login");
			}
			else
			{
				Errors = result.Errors;
				ShowErrors = true;
			}
	}

}
```

The register component contains a form which allows the user to enter their email address and desired password. When the form is submitted the ```Register``` method on the ```AuthService``` is called passing in the ```RegisterModel```. If the result of the registration is a success then the user is navigated to the login page. Otherwise any errors are displayed to the user.

### Login Compoenent

Now we can register a new account, we need to be able to login. The login component is going to be responsible for that.

```html
@page "/login"
@inject IAuthService AuthService
@inject NavigationManager NavigationManager

<h1>Login</h1>

@if (ShowErrors)
{
    <div class="alert alert-danger" role="alert">
        <p>@Error</p>
    </div>
}

<div class="card">
    <div class="card-body">
        <h5 class="card-title">Please enter your details</h5>
        <EditForm Model="loginModel" OnValidSubmit="HandleLogin">
            <DataAnnotationsValidator />
            <ValidationSummary />

            <div class="form-group">
                <label for="email">Email address</label>
                <InputText Id="email" Class="form-control" @bind-Value="loginModel.Email" />
                <ValidationMessage For="@(() => loginModel.Email)" />
            </div>
            <div class="form-group">
                <label for="password">Password</label>
                <InputText Id="password" type="password" Class="form-control" @bind-Value="loginModel.Password" />
                <ValidationMessage For="@(() => loginModel.Password)" />
            </div>
            <button type="submit" class="btn btn-primary">Submit</button>
        </EditForm>
    </div>
</div>

@code {

    private LoginModel loginModel = new LoginModel();
    private bool ShowErrors;
    private string Error = "";

    private async Task HandleLogin()
    {
        ShowErrors = false;

        var result = await AuthService.Login(loginModel);

        if (result.Successful)
        {
            NavigationManager.NavigateTo("/");
        }
        else
        {
            Error = result.Error;
            ShowErrors = true;
        }
    }

}
```

Following a similar design to the register component. There is a form for the user to input their email address and password.

When the form is submitted the ```AuthService``` is called and the result is returned. If the login was successful then the user is redirected to the home page, otherwise they are shown the error message.

### Logout Component

We can now register and login but we also need the ability to logout. I've gone with a page component to do this but you could also implement this on a button click somewhere.

```html
@page "/logout"
@inject IAuthService AuthService
@inject NavigationManager NavigationManager

@code {

    protected override async Task OnInitializedAsync()
    {
        await AuthService.Logout();
        NavigationManager.NavigateTo("/");
    }

}
```

The component doesn't have any UI, when the user navigates to it the ```Logout``` method on the ```AuthService``` is called and then the user is redirected back to the home page.

### Adding a LoginDisplay and updating the MainLayout

The final task is to add a ```LoginDisplay``` component and then update the ```MainLayout``` component to use it.

The ```LoginDisplay``` component is the same one used in the server-side Blazor template. If unauthenticated, it shows the Register and Log in links - if authenticated, it shows the users email and the Log out link. This component should be created in the ```Shared``` folder under the Client project.

```html
<AuthorizeView>
    <Authorized>
        Hello, @context.User.Identity.Name!
        <a href="LogOut">Log out</a>
    </Authorized>
    <NotAuthorized>
        <a href="Register">Register</a>
        <a href="Login">Log in</a>
    </NotAuthorized>
</AuthorizeView>
```

We just need to update the MainLayout component now to look like the code below.

```html
@inherits LayoutComponentBase

<div class="page">
	<div class="sidebar">
		<NavMenu />
	</div>

	<main>
		<div class="top-row px-4">
			<LoginDisplay />
			<a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
		</div>

		<article class="content px-4">
			@Body
		</article>
	</main>
</div>

```

### Registering Services

The last thing that's needed is to register the various services we've been building in the ```Program.cs``` file of the Client project. Add these to the end of the added services code.

```c#
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
```

If everything has gone to plan then you should now have a client-side Blazor app with authentication.

## Summary

In this post I showed how to create a new Blazor client-side application with authentication using WebAPI and ASP.NET Core Identity.

I showed how to configure the API to process and issue JSON web tokens. As well as how to setup the various controller actions to service the client application.I then showed how to configure Blazor to use the API and the tokens it issued to set the apps authentication state.
