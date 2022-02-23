# Authentication with client-side Blazor using WebAPI and ASP.NET Core Identity

## Getting Setup: Creating the solution

![Create Blazor Solution Step 1](https://github.com/scottmwalton/code-samples/blob/master/BlazorWasmDemo/assets/images/create_blazor_solution1.png "Create Blazor Solution Step 1")

Start by creating a new Blazor WebAssembly App, this template will create a Blazor application which runs in the 
clients browser on WebAssembly hosted by a ASP.NET Core WebAPI. Search for "Blazor" templates if it does not appear in the list.

![Create Blazor Solution Step 2](/assets/images/create_blazor_solution2.png "Create Blazor Solution Step 2")

Fill in the appropriate values for Project Name, Location and Solution Name

![Create Blazor Solution Step 3](/assets/images/create_blazor_solution3.png "Create Blazor Solution Step 3")

On the Additional information window, remember to tick the ASP.NET Core hosted checkbox. Eventhough we are creating an app
with Authentication, make sure to select "None" for the Authentication type. Once the solution has been created we're going 
to start making some changes to the server project.

## Configuring WebAPI

We're goint to configure the API first, but before we begin let's get some NuGet packages installed.

'''
		\<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.2" />
		\<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="6.0.2" />
		\<PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.2" />
		\<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.2">
			\<PrivateAssets>all</PrivateAssets>
			\<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		\</PackageReference>
		\<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.2" />
		\<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.2">
			\<PrivateAssets>all</PrivateAssets>
			\<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		\</PackageReference>
'''
