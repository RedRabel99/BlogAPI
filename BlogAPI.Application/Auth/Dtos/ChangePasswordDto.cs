namespace BlogAPI.Application.Auth.Dtos;

public class ChangePasswordDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}