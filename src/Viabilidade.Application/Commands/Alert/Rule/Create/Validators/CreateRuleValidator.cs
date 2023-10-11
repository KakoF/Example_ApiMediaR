using FluentValidation;
using Viabilidade.Domain.Interfaces.Repositories.Alert;
using Viabilidade.Domain.Interfaces.Services.Alert;
using Viabilidade.Domain.Interfaces.Services.Org;

namespace Viabilidade.Application.Commands.Alert.Rule.Create.Validators
{
    public class CreateRuleValidator : AbstractValidator<CreateRuleRequest>
    {
        private readonly IIndicatorRepository _indicadorRepository;
        private readonly IOperatorRepository _operadorRepository;
        private readonly IAlgorithmRepository _algoritmoTipoRepository;
        private readonly IEntityService _entidadeService;
        private readonly ITagService _tagService;
        public CreateRuleValidator(IIndicatorRepository indicadorRepository, IOperatorRepository operadorRepository, IAlgorithmRepository algoritmoTipoRepository, IEntityService entidadeService, ITagService tagService)
        {
            _indicadorRepository = indicadorRepository;
            _operadorRepository = operadorRepository;
            _algoritmoTipoRepository = algoritmoTipoRepository;
            _entidadeService = entidadeService;
            _tagService = tagService;

            RuleFor(a => a.Name)
                .NotEmpty().WithMessage("Nome não pode ser vazio");

            RuleFor(a => a.AlgorithmId)
                .NotEmpty().WithMessage("Algoritimo não pode ser vazio")
                .MustAsync(async (value, c) => await TipoExistAsync(value)).WithMessage((value) => $"Tipo {value.AlgorithmId} não encontrado");

            RuleFor(a => a.IndicatorId)
                .NotEmpty().WithMessage("Indicador não pode ser vazio")
                .MustAsync(async (value, c) => await IndicadorExistAsync(value)).WithMessage((value) => $"Indicador {value.IndicatorId} não encontrado");

            RuleFor(a => a.OperatorId)
                .NotEmpty().WithMessage("Operador não pode ser vazio")
                .MustAsync(async (value, c) => await OperadorExistAsync(value)).WithMessage((value)  => $"Operador {value.OperatorId} não encontrado");

            RuleFor(a => a.Active)
                .NotEmpty().WithMessage("Ativo não pode ser vazio");

            RuleFor(a => a.Pinned)
                .NotEmpty().WithMessage("Favorito não pode ser vazio");

            RuleFor(a => a.Parameter)
                .NotEmpty().WithMessage("Parametro não pode ser vazio");

            RuleFor(a => a.Tags)
               .Must(x => x.Select(c => c.Id).Distinct().Count() == x.Count()).WithMessage("Existem tags duplicadas")
               .MustAsync(async (value, c) => await TagExistAsync(value)).WithMessage("Tag não encontrada");

            RuleFor(a => a.EntityRules)
              .NotEmpty().WithMessage("Vínculo de squads/entidades/canais não pode ser vazio")
              .Must(x => x.Select(c => c.EntityId).Distinct().Count() == x.Count()).WithMessage("Existem entidades duplicadas")
              .ForEach(vinculos =>
              {
                  vinculos.MustAsync(async (value, c) => await EntidadeExistAsync(value))
                 .WithMessage((value, c) => $"Entidade {c.EntityId} não encontrada");
              });
        }

        private async Task<bool> TipoExistAsync(int id)
        {
            var tipo = await _algoritmoTipoRepository.GetAsync(id);
            if (tipo == null || !tipo.Active)
                return false;
            return true;
        }
        private async Task<bool> IndicadorExistAsync(int id)
        {
            var indicador = await _indicadorRepository.GetAsync(id);
            if (indicador == null || !indicador.Active)
                return false;
            return true;
        }

        private async Task<bool> OperadorExistAsync(int id)
        {
            var operador = await _operadorRepository.GetAsync(id);
            if (operador == null || !operador.Active)
                return false;
            return true;
        }

        private async Task<bool> TagExistAsync(IEnumerable<CreateTagsRequest> tags)
        {
            foreach (var t in tags)
            {
                var tag = await _tagService.GetAsync(t.Id);
                if (tag == null || !tag.Active)
                    return false;
            }
            return true;
        }

        private async Task<bool> EntidadeExistAsync(CreateEntityRuleRequest regraEntidade)
        {
            var entidade = await _entidadeService.GetAsync((int)regraEntidade.EntityId);
            if (entidade == null || !entidade.Active)
                return false;
            return true;
        }
    }
}
