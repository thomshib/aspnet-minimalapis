https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0

1. xunit Test

dotnet new xunit -o Library.Api.Tests.Integration

2. Microsoft.NET.Test.Sdk
dotnet add package Microsoft.NET.Test.Sdk --version 17.5.0

3. FluentAssertions
dotnet add package FluentAssertions --version 6.10.0

4. Microsoft.AspNetCore.Mvc.Testing - Library for integration testing
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 7.0.3

5. Add reference to Library API

dotnet add Library.Api.Tests.Integration.csproj reference ../../src/Library.Api.csproj
