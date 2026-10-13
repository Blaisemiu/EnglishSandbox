using System.ComponentModel.DataAnnotations;

namespace EnglishSandbox.Api.Auth;

public class LoginRequest
{
    [Required]
    public string Login { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}
