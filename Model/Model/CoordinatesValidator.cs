using FluentValidation;

namespace Domain.Model
{
  public class CoordinatesRequestValidator : AbstractValidator<Coordinates>
    {
        public CoordinatesRequestValidator() 
        {
            RuleFor(s => s.latitude)
             .NotEmpty().WithMessage("Latitude é obrigatório.")
             .NotNull().WithMessage("Latitude é obrigatório.");
            RuleFor(s => s.longitude)
             .NotEmpty().WithMessage("Longitude é obrigatório.")
             .NotNull().WithMessage("Longitude é obrigatório.");
            
        }
    }
}
