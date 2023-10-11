using MediatR;
using Viabilidade.Domain.Interfaces.Services.Alert;
using Viabilidade.Domain.Models.Alert;
using Viabilidade.Infrastructure.Interfaces.DataConnector;

namespace Viabilidade.Application.Commands.Alert.Rule.Create
{
    public class CreateCommandHandler : IRequestHandler<CreateRuleRequest, RuleModel>
    {
        private readonly IRuleService _regraAlertaService;
        private readonly ITagAlertService _alertaTagService;
        private readonly IFavoriteAlertService _alertaFavoritoService;
        private readonly IParameterService _parametroService;
        private readonly IRChannelEntityRuleService _rRegraEntidadeCanalService;
        private readonly IEntityRuleService _regraEntidadeService;
        private readonly IUnitOfWork _unitOfWork;
        public CreateCommandHandler(IRuleService regraAlertaService, ITagAlertService alertaTagService, IFavoriteAlertService alertaFavoritoService, IParameterService parametroService, IRChannelEntityRuleService rRegraEntidadeCanalService, IEntityRuleService regraEntidadeService, IUnitOfWork unitOfWork)
        {
            _regraAlertaService = regraAlertaService;
            _alertaTagService = alertaTagService;
            _alertaFavoritoService = alertaFavoritoService;
            _parametroService = parametroService;
            _rRegraEntidadeCanalService = rRegraEntidadeCanalService;
            _regraEntidadeService = regraEntidadeService;
            _unitOfWork = unitOfWork;
        }
        public async Task<RuleModel> Handle(CreateRuleRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var parametro = await ParametroCreateAsync(request.Parameter);
                var regra = await RegraAlertaCreateAsync(parametro, request);
                var tags = await AlertaTagsCreateAsync(regra.Id, request.Tags);
                if (request.Pinned)
                    await FavoritarRegraAsync(regra);
                await EntidadeRegraCreateAsync(regra.Id ,request.EntityRules);

                _unitOfWork.CommitTransaction();

                var response = regra;
                regra.TagAlerts = tags;
                regra.Parameter = parametro;

                return response;
            }
            catch
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        private async Task EntidadeRegraCreateAsync(int regraId, IEnumerable<CreateEntityRuleRequest> vinculosIncluidos)
        {
            foreach (var vinculos in vinculosIncluidos)
            {
                ParameterModel parametro = null;
                if (vinculos.Parameter != null)
                    parametro = await ParametroCreateAsync(vinculos.Parameter);

                var regraEntidade = new EntityRuleModel()
                {
                    RuleId = regraId,
                    EntityId = vinculos.EntityId,
                    Active = vinculos.Active,
                    ParameterId = parametro?.Id
                };

                var regra = await _regraEntidadeService.CreateAsync(regraEntidade);
                if (vinculos?.ChannelId != null)
                    await _rRegraEntidadeCanalService.CreateAsync(new RChannelEntityRuleModel { EntityRuleId = regra.Id, ChannelId = vinculos.ChannelId });
            }
        }

        private async Task<ParameterModel> ParametroCreateAsync(CreateParameterRequest parametroRequest)
        {
            var parametro = new ParameterModel()
            {
                Active = true,
                HighSeverity = parametroRequest.HighSeverity,
                MediumSeverity = parametroRequest.MediumSeverity,
                LowSeverity = parametroRequest.LowSeverity,
                ComparativePeriod = parametroRequest.ComparativePeriod,
                EvaluationPeriod = parametroRequest.EvaluationPeriod
            };
            return await _parametroService.CreateAsync(parametro);
        }

        private async Task FavoritarRegraAsync(RuleModel regra)
        {
            var favorito = new FavoriteAlertModel() { RuleId = regra.Id };
            await _alertaFavoritoService.FavoriteAsync(favorito);
        }

        private async Task<IEnumerable<TagAlertModel>> AlertaTagsCreateAsync(int id, IEnumerable<CreateTagsRequest> tags)
        {
            var list = new List<TagAlertModel>();
            foreach (var tag in tags)
            {
                list.Add(await _alertaTagService.CreateAsync(new TagAlertModel() { RuleId = id, TagId = tag.Id, Active = true }));
            }
            return list;
        }

        private async Task<RuleModel> RegraAlertaCreateAsync(ParameterModel parametro, CreateRuleRequest request)
        {
            var regra = new RuleModel() { Name = request.Name, Description = request.Description, AlgorithmId = request.AlgorithmId, IndicatorId = request.IndicatorId, OperatorId = request.OperatorId, ParameterId = parametro.Id, Active = request.Active, LastUpdateDate = DateTime.Now };
            regra.NewVersion();
            return await _regraAlertaService.CreateAsync(regra);
        }

    }
}