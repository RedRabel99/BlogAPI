using BlogAPI.Application.Auth.Dtos;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators;

public class LogoutRequestDtoValidator : AbstractValidator<LogoutRequestDto>
{
    public LogoutRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
