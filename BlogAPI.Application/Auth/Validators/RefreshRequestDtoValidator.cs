using BlogAPI.Application.Auth.Dtos;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators;

public class RefreshRequestDtoValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
