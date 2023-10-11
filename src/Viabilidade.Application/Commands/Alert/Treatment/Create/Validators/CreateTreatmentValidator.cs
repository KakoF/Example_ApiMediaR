using FluentValidation;
using Viabilidade.Application.Commands.Alert.Treatment.Create;
using Viabilidade.Domain.Interfaces.Services.Alert;

namespace Viabilidade.Application.Commands.Alert.Rule.Create.Validators
{
    public class CreateTreatmentValidator : AbstractValidator<CreateTreatmentRequest>
    {
        private readonly IAlertService _alertaGeradoService;
        private readonly ITreatmentClassService _classeTratativaService;
        private readonly ITreatmentTypeService _tipoTratativaService;
        
        public CreateTreatmentValidator(IAlertService alertaGeradoService, ITreatmentClassService classeTratativaService, ITreatmentTypeService tipoTratativaService)
        {
            _alertaGeradoService = alertaGeradoService;
            _classeTratativaService = classeTratativaService;
            _tipoTratativaService = tipoTratativaService;
           

            RuleFor(a => a.AlertIds)
              .NotEmpty().WithMessage("Alerta não pode ser vazio")
              .Must(x => x.Select(c => c).Distinct().Count() == x.Count()).WithMessage("Existem alertas duplicados")
              .ForEach(vinculos =>
              {
                  vinculos.MustAsync(async (value, c) => await AlertasExistAsync(value))
                 .WithMessage((value, c) => $"Alerta {c} não encontrado");
              });

             RuleFor(a => a.TreatmentClassId)
              .NotEmpty().WithMessage("Tratativa não pode ser vazia")
              .MustAsync(async (value, c) => await TratativaExistAsync(value)).WithMessage("Tratativa não encontrada");

             RuleFor(a => a.TreatmentTypeId)
              .NotEmpty().WithMessage("Tratativa não pode ser vazia")
              .MustAsync(async (value, c) => await TipoTratativaExistAsync(value)).WithMessage("Tratativa não encontrada");

             RuleFor(a => a.Description)
              .NotEmpty().WithMessage("Descrição não pode ser vazia");

            RuleFor(a => a)
                .Must(AlertaSilenciado).WithMessage("Dias silenciado não poder ser vazio");

        }

        private async Task<bool> AlertasExistAsync(int alertaId)
        {
            var alerta = await _alertaGeradoService.GetAsync(alertaId);
            if (alerta == null || alerta.Active == false || alerta.Treated == true)
                return false;
            return true;
        }

        private async Task<bool> TratativaExistAsync(int tratativaId)
        {
            var tratativa = await _classeTratativaService.GetAsync(tratativaId);
            if(tratativa == null || !tratativa.Active)
                return false;

            return true;
        }

        private async Task<bool> TipoTratativaExistAsync(int tipoTratativaId)
        {
            var tipoTratativa = await _tipoTratativaService.GetAsync(tipoTratativaId);
            if (tipoTratativa == null || !tipoTratativa.Active)
                return false;

            return true;
        }

        private bool AlertaSilenciado(CreateTreatmentRequest tratativa)
        {
            if (tratativa.Mute == true && tratativa.MuteDays == null)
                return false;
            return true;
        }


    }
}
