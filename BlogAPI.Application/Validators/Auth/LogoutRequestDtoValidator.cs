using BlogAPI.Application.DTOs.Auth;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class LogoutRequestDtoValidator : AbstractValidator<LogoutRequestDto>
{
    public LogoutRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
