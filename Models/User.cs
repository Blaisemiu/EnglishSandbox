namespace EnglishSandbox.Api.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; private set; } = "";

    public string Email { get; private set; } = "";

    public bool IsActive { get; private set; } = true;

    public void UpdateProfile(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя не должно быть пустым.");

        if (string.IsNullOrWhiteSpace(email) ||
            !System.Net.Mail.MailAddress.TryCreate(email.Trim(), out var address) ||
            address.Address != email.Trim())
        {
            throw new ArgumentException("Укажи корректный email.");
        }

        Name = name.Trim();
        Email = email.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}