namespace ISO11820.Models;

public class Operator
{
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string UserType { get; set; } = ""; // admin / operator
}