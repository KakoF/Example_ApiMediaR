using MediatR;
using Viabilidade.Domain.Exceptions;
using Viabilidade.Domain.Interfaces.Services.Alert;
using Viabilidade.Domain.Models.Alert;
using Viabilidade.Infrastructure.Interfaces.DataConnector;

namespace Viabilidade.Application.Commands.Alert.Rule.Update
{
    public class UpdateCommandHandler : IRequestHandler<UpdateRuleRequest, RuleModel>
    {
        private readonly IRuleService _regraAlertaService;
        private readonly ITagAlertService _alertaTagService;
        private readonly IParameterService _parametroService;
        private readonly IRChannelEntityRuleService _rRegraEntidadeCanalService;
        private readonly IEntityRuleService _regraEntidadeService;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCommandHandler(IRuleService regraAlertaService, ITagAlertService alertaTagService, IParameterService parametroService, IRChannelEntityRuleService rRegraEntidadeCanalService, IEntityRuleService regraEntidadeService, IUnitOfWork unitOfWork)
        {
            _regraAlertaService = regraAlertaService;
            _alertaTagService = alertaTagService;
            _parametroService = parametroService;
            _rRegraEntidadeCanalService = rRegraEntidadeCanalService;
            _regraEntidadeService = regraEntidadeService;
            _unitOfWork = unitOfWork;
        }
        public async Task<RuleModel> Handle(UpdateRuleRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var regra = await _regraAlertaService.GetAsync(request.Id);
                if (regra == null)
                    throw new DomainException("Regra não encontrada");
                regra.Parameter = await _parametroService.GetAsync((int)regra.ParameterId);

                _unitOfWork.BeginTransaction();

                var parametro = await ParametroUpdateAsync(regra.Parameter, request.Parameter);
                var regraUpdate = await RegraAlertaUpdateAsync(regra, parametro, request);
                var tags = await AlertaTagsUpdateAsync(regra.Id, request.Tags);

                await EntidadeRegraUpdateAsync(regraUpdate, request.UpdateEntityRules);
                await EntidadeRegraCreateAsync(regra.Id, request.CreateEntityRules);

                _unitOfWork.CommitTransaction();

                var response = regraUpdate;
                regraUpdate.TagAlerts = tags;
                regraUpdate.Parameter = parametro;

                return response;
               
            }
            catch
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }


        private async Task<ParameterModel> ParametroUpdateAsync(ParameterModel parametro, ParameterRequest parametroRequest)
        {
            parametro.Active = true;
            parametro.HighSeverity = parametroRequest.HighSeverity;
            parametro.MediumSeverity = parametroRequest.MediumSeverity;
            parametro.LowSeverity = parametroRequest.LowSeverity;
            parametro.ComparativePeriod = parametroRequest.ComparativePeriod;
            parametro.EvaluationPeriod = parametroRequest.EvaluationPeriod;
            return await _parametroService.UpdateAsync(parametro.Id, parametro);
        }


        private async Task EntidadeRegraUpdateAsync(RuleModel regra, IEnumerable<EntityRuleRequest> updateVinculos)
        {
            var regraEntidades = await _regraEntidadeService.GetByRuleAsync(regra.Id);
            foreach (var regraEntidade in regraEntidades)
            {
                var update = updateVinculos.FirstOrDefault(x => x.EntityId == regraEntidade.EntityId);
                var parametroId = await UpdateParametroAsync(regraEntidade.ParameterId, update.Parameter, update.Active);
                regraEntidade.Active = update.Active;
                regraEntidade.ParameterId = parametroId;
                await _regraEntidadeService.UpdateAsync(regraEntidade.Id, regraEntidade);
            }
        }

        private async Task<int?> UpdateParametroAsync(int? parameterId, ParameterRequest parameter, bool active)
        {
            if (parameterId == null && parameter == null)
                return null;

            if (parameterId == null)
            {
                var newParameter = new ParameterModel()
                {
                    Active = active,
                    HighSeverity = parameter.HighSeverity,
                    MediumSeverity = parameter.MediumSeverity,
                    LowSeverity = parameter.LowSeverity,
                    ComparativePeriod = parameter.ComparativePeriod,
                    EvaluationPeriod = parameter.EvaluationPeriod
                };
                return (await _parametroService.CreateAsync(newParameter)).Id;
            }

            var parametro = await _parametroService.GetAsync((int)parameterId);
            parametro.Active = active;
            return (await _parametroService.UpdateAsync((int)parameterId, parametro)).Id;
        }

        private async Task EntidadeRegraCreateAsync(int regraId, IEnumerable<EntityRuleRequest> vinculosIncluidos)
        {
            if (vinculosIncluidos == null)
                return;

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


        private async Task<ParameterModel> ParametroCreateAsync(ParameterRequest parametroRequest)
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

        private async Task<IEnumerable<TagAlertModel>> AlertaTagsUpdateAsync(int id, IEnumerable<UpdateTagsRequest> tags)
        {
            await _alertaTagService.DeleteByRuleAsync(id);
            var list = new List<TagAlertModel>();
            foreach (var tag in tags)
            {
                list.Add(await _alertaTagService.CreateAsync(new TagAlertModel() { RuleId = id, TagId = tag.Id, Active = true }));
            }
            return list;
        }

        private async Task<RuleModel> RegraAlertaUpdateAsync(RuleModel regra, ParameterModel parametro, UpdateRuleRequest request)
        {
            var update = CompleteObject(parametro, request);
            update.UpdateVersion(regra);
            return await _regraAlertaService.UpdateAsync(request.Id, update);
        }

        private RuleModel CompleteObject(ParameterModel parametro, UpdateRuleRequest request)
        {
            var tags = request.Tags.Select(a => new TagAlertModel() { Id = a.Id });
            var complete = new RuleModel()
            {
                Name = request.Name,
                Description = request.Description,
                AlgorithmId = request.AlgorithmId,
                IndicatorId = request.IndicatorId,
                OperatorId = request.OperatorId,
                ParameterId = parametro.Id,
                Active = request.Active,
                LastUpdateDate = DateTime.Now,
                Parameter = parametro,
                TagAlerts = tags
            };
            return complete;


        }

    }
}