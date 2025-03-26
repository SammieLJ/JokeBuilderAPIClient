# Joke Builder API Client
API Client for fetching and storing jokes from Random Joke API

API url (use browser or curl)
https://official-joke-api.appspot.com/random_joke

Task that Users should be able to:
* 1.) Fetch jokes (specify the number).
* 2.) View all stored jokes.
* 3.) Search jokes by type.
* 4.) Process multiple jokes (remove N jokes at once).
* 5.) Exit.

And my solution is in Programs.cs. Use "dotnet new console -n JokeBuilder"  and copy&paste code. 

You will need Newtonsoft.Json package, add it:
"dotnet add package Newtonsoft.Json --version 13.0.3"

More info about package: https://www.nuget.org/packages/newtonsoft.json/

Did it, as dailly practice to keep with programming skills, minimum usage of GitHub CoPilot in VS Code

Update:
Made more compelx with test cases. Cleaned some bits, static helper methods are now type Task for more thread safety. Didn't add libraries, cached objects and exe files, because you can build it locally, no need to spam github. Instructions are bellow, how to run.

After cloning repository, you should be in command prompt or teminal, set to "JokeBuilderAPIClient" folder, that would be working root folder. You should have .Net 8 installed (check with cmd dotnet --version) and dotnet command should work.

Add these NuGet packages (as cmd commands):
* dotnet add package xunit
* dotnet add package Newtonsoft.Json
* dotnet add package Microsoft.NET.Test.Sdk
* dotnet add package xunit.runner.visualstudio

To try out:
- dotnet clean
- dotnet build
- dotnet run --project JokeProcessing
- dotnet test

To Run Tests Efficiently:
- dotnet test --filter "FullyQualifiedName~JokeService.Tests"
