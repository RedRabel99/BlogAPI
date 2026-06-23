using BlogAPI.Application.DTOs.Auth;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class RefreshRequestDtoValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
