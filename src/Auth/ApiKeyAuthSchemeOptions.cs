
using Microsoft.AspNetCore.Authentication;



public class ApiKeyAuthSchemeOptions: AuthenticationSchemeOptions{

    public string  ApiKey { get; set; } = "VerySecret";


}