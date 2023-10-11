using Dapper;
using Microsoft.AspNetCore.Http;
using Viabilidade.Domain.DTO.Alert;
using Viabilidade.Domain.Models.Alert;
using Viabilidade.Domain.Interfaces.Repositories.Alert;
using Viabilidade.Domain.Models.QueryParams.Alert;
using Viabilidade.Infrastructure.ContextAccessor;
using Viabilidade.Infrastructure.Interfaces.DataConnector;

namespace Viabilidade.Infrastructure.Repositories.Alertas
{
    public class AlertRepository : UserContextAccessor, IAlertRepository
    {
        private readonly IDbConnector _connector;
        public AlertRepository(IDbConnector connector, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _connector = connector;
        }
        public async Task<AlertModel> CreateAsync(AlertModel entity)
        {
            entity.Id = await _connector.dbConnection.QuerySingleAsync<int>("INSERT INTO Alertas.AlertaGerado" +
                "(EntidadeId, EntidadeNome, RegraAlertaEntidadeId, RegraAlertanome, Versao, DiaMinIndicador, DiaMaxIndicador, Criticidade, Indicador, IndicadorReferenciabaixo, IndicadorReferenciamedio, IndicadorReferenciaalto, AlertaStatusId, DataFinalizacao, UsuarioIdResponsavel, Ativo, Tratado, ValorIndicador, PorcentagemIndicador) OUTPUT Inserted.Id " +
                "VALUES" +
                "(@EntityId, @EntityName, @EntityRuleId, @RuleName, @Version, @IndicatorFirstDate, @IndicadorLastDate, @Severity, @Indicator, @LowReferenceIndicator, @MediumReferenceIndicator, @HighReferenceIndicator, @StatusId, @FinishDate, @UsuarioIdResponsavel, @Active, @Treated, @IndicatorValue, @PercentageIndicator)"
                , new { entity.EntityId, entity.EntityName, entity.EntityRuleId, entity.RuleName, entity.Version, entity.IndicatorFirstDate, entity.IndicadorLastDate, entity.Severity, entity.Indicator, entity.LowReferenceIndicator, entity.MediumReferenceIndicator, entity.HighReferenceIndicator, entity.StatusId, entity.FinishDate, UsuarioIdResponsavel = _userId, entity.Active, entity.Treated, entity.IndicatorValue, entity.PercentageIndicator }, _connector.dbTransaction);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return Convert.ToBoolean(await _connector.dbConnection.ExecuteAsync("DELETE FROM Alertas.AlertaGerado where Id = @Id", new { id }, _connector.dbTransaction));
        }

        public async Task<IEnumerable<AlertModel>> GetAsync()
        {
            return await _connector.dbConnection.QueryAsync<AlertModel>("Select Id, EntidadeId As EntityId, EntidadeNome as EntityName, RegraAlertaEntidadeId as EntityRuleId, RegraAlertanome as RuleName, Versao as Version, DiaMinIndicador as IndicatorFirstDate, DiaMaxIndicador as IndicadorLastDate, Criticidade as Severity, Indicador as Indicator, IndicadorReferenciabaixo as LowReferenceIndicator, IndicadorReferenciamedio as MediumReferenceIndicator, IndicadorReferenciaalto as HighReferenceIndicator, AlertaStatusId as StatusId, DataFinalizacao as FinishDate, UsuarioIdResponsavel as UserId, Ativo as Active, Tratado as Treated, ValorIndicador as IndicatorValue, PorcentagemIndicador as PercentageIndicator from Alertas.AlertaGerado");

        }

        public async Task<IEnumerable<AlertModel>> GetByEntityRuleAsync(int entityRuleId, bool? treated)
        {
            return await _connector.dbConnection.QueryAsync<AlertModel>($"Select Id, EntidadeId As EntityId, EntidadeNome as EntityName, RegraAlertaEntidadeId as EntityRuleId, RegraAlertanome as RuleName, Versao as Version, DiaMinIndicador as IndicatorFirstDate, DiaMaxIndicador as IndicadorLastDate, Criticidade as Severity, Indicador as Indicator, IndicadorReferenciabaixo as LowReferenceIndicator, IndicadorReferenciamedio as MediumReferenceIndicator, IndicadorReferenciaalto as HighReferenceIndicator, AlertaStatusId as StatusId, DataFinalizacao as FinishDate, UsuarioIdResponsavel as UserId, Ativo as Active, Tratado as Treated, ValorIndicador as IndicatorValue, PorcentagemIndicador as PercentageIndicator from Alertas.AlertaGerado where regraAlertaEntidadeId = @entityRuleId {(treated != null ? "and Tratado = @treated" : "and Tratado is null")}", new { entityRuleId, treated }, _connector.dbTransaction);

        }

        public async Task<int> CountLowSeverityByEntityRuleAsync(int entityRuleId)
        {
            return await _connector.dbConnection.ExecuteScalarAsync<int>($"Select Count(Id) From Alertas.AlertaGerado where regraAlertaEntidadeId = @entityRuleId", new { entityRuleId }, _connector.dbTransaction);

        }

        public async Task<IEnumerable<AlertModel>> GetAsync(IEnumerable<int> ids)
        {
            return await _connector.dbConnection.QueryAsync<AlertModel>("Select Id, EntidadeId As EntityId, EntidadeNome as EntityName, RegraAlertaEntidadeId as EntityRuleId, RegraAlertanome as RuleName, Versao as Version, DiaMinIndicador as IndicatorFirstDate, DiaMaxIndicador as IndicadorLastDate, Criticidade as Severity, Indicador as Indicator, IndicadorReferenciabaixo as LowReferenceIndicator, IndicadorReferenciamedio as MediumReferenceIndicator, IndicadorReferenciaalto as HighReferenceIndicator, AlertaStatusId as StatusId, DataFinalizacao as FinishDate, UsuarioIdResponsavel as UserId, Ativo as Active, Tratado as Treated, ValorIndicador as IndicatorValue, PorcentagemIndicador as PercentageIndicator from Alertas.AlertaGerado where id in @ids", new { ids }, _connector.dbTransaction);

        }

        public async Task<IEnumerable<Tuple<AlertGroupDto, int>>> GroupAsync(AlertQueryParams queryParams)
        {
            return await _connector.dbConnection.QueryAsync<AlertGroupDto, int, Tuple<AlertGroupDto, int>>("select " +
               "Max(ag.Id) as Id, " +
               "Max(e.Nome) as EntityName, " +
               "agc.CanalNome as ChannelName, " +
               "ra.Nome as AlertName, " +
               "Max(ag.Indicador) as Value, " +
               "Count(ag.RegraAlertaEntidadeId) AlertsQuantity, " +
               "Max(ag.Criticidade) as Severity, " +
               "Max(ag.Versao) as Date, " +
               "(Convert(varchar(3),ra.VersaoMajor,0) + '.' + Convert(varchar(3),ra.VersaoMinor,0) + '.' + Convert(varchar(3),ra.VersaoPatch,0)) As Version, " +
               "ag.RegraAlertaEntidadeId As EntityRuleId, " +
               "(count (*) over (partition by 1)) AS Total " +
               "from Alertas.AlertaGerado ag " +
                   "inner join Org.Entidade e on ag.EntidadeId = e.Id " +
                   "inner join Alertas.RegraEntidade re on ag.RegraAlertaEntidadeId = re.Id " +
                   "inner join Alertas.RegraAlerta ra on re.AlertaId = ra.Id " +
                   $"{(queryParams.Pinned ?? false ? " cross apply(SELECT Top (1) favorito.Id from Alertas.AlertaFavorito favorito where favorito.AlertaId = ra.id AND favorito.UsuarioId = @userId and favorito.Ativo = 1) as af" : "")} " +
                   $"{(queryParams.Tags != null ? "outer apply(SELECT Top (1) tag.TagId from Alertas.AlertaTag tag where tag.RegraAlertaId = ra.id AND (tag.TagId IN @tags)) as t " : "")} " +
                   "left join Org.R_SquadEntidade rse on re.EntidadeId = rse.EntidadeId " +
                   "left join Org.Squad s on rse.SquadId = s.Id " +
                   $"outer apply(SELECT Top (1) canal.CanalId, canal.CanalNome from Alertas.AlertaGeradoCanal canal where canal.AlertaGeradoId = ag.id {(queryParams.Channels != null ? "AND (canal.CanalId IN @canais)" : "")}) as agc " +
                   "where ag.Tratado is null " +
               $"{(!string.IsNullOrEmpty(queryParams.Search) ? "AND (ag.EntidadeNome like @search OR e.Nome like @search OR ra.Nome like @search) " : "")} " +
               $"{(queryParams.Squads != null ? "AND (s.Id IN @squads) " : "")} " +
               $"{(queryParams.Entities != null ? "AND (e.Id IN @entidades OR ag.EntidadeId IN @entidades)  " : "")} " +
               $"{(queryParams.Responsibles != null ? "AND (ag.UsuarioIdResponsavel IN @responsaveis) " : "")} " +
               $"{(queryParams.Tags != null ? "AND (t.TagId IN @tags) " : "")} " +
               $"{(queryParams.Channels != null ? "AND (agc.CanalId IN @canais) " : "")} " +
               $"{(queryParams.InitialDate != null ? " AND ag.Versao >= @initialDate " : "")} " +
               $"{(queryParams.FinalDate != null ? " AND ag.Versao <= @finalDate " : "")} " +
               $"{(queryParams.Active != null ? " AND ag.Ativo = @ativo " : "")} " +
               "group by " +
                   "ag.RegraAlertaEntidadeId, " +
                   "agc.CanalNome, " +
                   "ra.Nome, " +
                   "ra.VersaoMajor, " +
                   "ra.VersaoMinor, " +
                   "ra.VersaoPatch " +
               $"ORDER BY {(int)queryParams.OrderCollumn + 1} {queryParams.SortOrders} " +
               "OFFSET @perPage * (@page - 1) ROWS FETCH NEXT @perPage ROWS ONLY",
              map: (alerta, tuple) =>
              {
                  return new(alerta, tuple);
              },
              param: new
              {
                  perPage = queryParams.TotalPage,
                  squads = queryParams.Squads,
                  entidades = queryParams.Entities,
                  responsaveis = queryParams.Responsibles,
                  tags = queryParams.Tags,
                  canais = queryParams.Channels,
                  queryParams.Page,
                  ativo = queryParams.Active,
                  search = ("%" + queryParams.Search?.ToLower() + "%"),
                  userId = _userId,
                  initialDate = queryParams.InitialDate,
                  finalDate = queryParams.FinalDate != null ? queryParams.FinalDate.Value.AddDays(1): queryParams.FinalDate,
              },
              _connector.dbTransaction,
              splitOn: "Total");
        }

        public async Task<AlertModel> GetAsync(int id)
        {
            return await _connector.dbConnection.QueryFirstOrDefaultAsync<AlertModel>("Select Id, EntidadeId As EntityId, EntidadeNome as EntityName, RegraAlertaEntidadeId as EntityRuleId, RegraAlertanome as RuleName, Versao as Version, DiaMinIndicador as IndicatorFirstDate, DiaMaxIndicador as IndicadorLastDate, Criticidade as Severity, Indicador as Indicator, IndicadorReferenciabaixo as LowReferenceIndicator, IndicadorReferenciamedio as MediumReferenceIndicator, IndicadorReferenciaalto as HighReferenceIndicator, AlertaStatusId as StatusId, DataFinalizacao as FinishDate, UsuarioIdResponsavel as UserId, Ativo as Active, Tratado as Treated, ValorIndicador as IndicatorValue, PorcentagemIndicador as PercentageIndicator from Alertas.AlertaGerado where id = @id", new { id }, _connector.dbTransaction);
        }

        public async Task<AlertModel> GetLastByEntityRuleAsync(int entityRuleId)
        {
            return await _connector.dbConnection.QueryFirstOrDefaultAsync<AlertModel>("Select top (1) Id, EntidadeId As EntityId, EntidadeNome as EntityName, RegraAlertaEntidadeId as EntityRuleId, RegraAlertanome as RuleName, Versao as Version, DiaMinIndicador as IndicatorFirstDate, DiaMaxIndicador as IndicadorLastDate, Criticidade as Severity, Indicador as Indicator, IndicadorReferenciabaixo as LowReferenceIndicator, IndicadorReferenciamedio as MediumReferenceIndicator, IndicadorReferenciaalto as HighReferenceIndicator, AlertaStatusId as StatusId, DataFinalizacao as FinishDate, UsuarioIdResponsavel as UserId, Ativo as Active, Tratado as Treated, ValorIndicador as IndicatorValue, PorcentagemIndicador as PercentageIndicator from Alertas.AlertaGerado where regraAlertaEntidadeId = @entityRuleId order by Versao Desc", new { entityRuleId }, _connector.dbTransaction);
        }

        public async Task<AlertModel> UpdateAsync(int id, AlertModel entity)
        {
            entity.Id = id;
            await _connector.dbConnection.ExecuteAsync("UPDATE Alertas.AlertaGerado " +
              "SET EntidadeId=@EntidadeId, EntidadeNome=@EntidadeNome, RegraAlertaEntidadeId=@RegraAlertaEntidadeId, RegraAlertanome=@RegraAlertaNome, Versao=@Versao, DiaMinIndicador=@DiaMinIndicador, DiaMaxIndicador=@DiaMaxIndicador, Criticidade=@Criticidade, Indicador=@Indicador, IndicadorReferenciabaixo=@IndicadorReferenciaBaixo, IndicadorReferenciamedio=@IndicadorReferenciaMedio, IndicadorReferenciaalto=@IndicadorReferenciaAlto, AlertaStatusId=@AlertaStatusId, DataFinalizacao=@DataFinalizacao, UsuarioIdResponsavel=@UsuarioIdResponsavel, Ativo=@Ativo, Tratado=@Tratado, ValorIndicador=@ValorIndicador, PorcentagemIndicador=@PorcentagemIndicador " +
              "where id = @id", new { id, EntidadeId = entity.EntityId, EntidadeNome = entity.EntityName, RegraAlertaEntidadeId = entity.EntityRuleId, RegraAlertaNome = entity.RuleName, Versao = entity.Version, DiaMinIndicador = entity.IndicatorFirstDate, DiaMaxIndicador = entity.IndicadorLastDate, Criticidade = entity.Severity, Indicador = entity.Indicator, IndicadorReferenciaBaixo = entity.LowReferenceIndicator, IndicadorReferenciaMedio = entity.MediumReferenceIndicator, IndicadorReferenciaAlto = entity.HighReferenceIndicator, AlertaStatusId = entity.StatusId, DataFinalizacao = entity.FinishDate, UsuarioIdResponsavel = entity.UserId, Ativo = entity.Active, Tratado = entity.Treated, ValorIndicador = entity.IndicatorValue, PorcentagemIndicador = entity.PercentageIndicator }, _connector.dbTransaction);
            return entity;
        }

        public async Task<AlertModel> PreviewAsync(int id)
        {
            return (await _connector.dbConnection.QueryAsync<AlertModel, EntityRuleModel, ParameterModel, StatusModel, AlertModel>(
                "Select ag.Id, ag.EntidadeId as EntityId, ag.EntidadeNome as EntityName, ag.RegraAlertaEntidadeId as EntityRuleId, ag.RegraAlertanome as RuleName, ag.Versao as Version, ag.DiaMinIndicador as IndicatorFirstDate, ag.DiaMaxIndicador as IndicadorLastDate, ag.Criticidade as Severity, ag.Indicador as Indicator, ag.IndicadorReferenciabaixo as LowReferenceIndicator, ag.IndicadorReferenciamedio as MediumReferenceIndicator, ag.IndicadorReferenciaalto as HighReferenceIndicator, ag.AlertaStatusId as StatusId, ag.DataFinalizacao as FinishDate, ag.UsuarioIdResponsavel as UserId, ag.Ativo as Actice, ag.Tratado as Treated, ag.ValorIndicador as IndicatorValue, ag.PorcentagemIndicador as PercentageIndicator, " +
                "re.Id, re.AlertaId as RuleId, re.UsuarioIdResponsavel as UserId, re.ParametroId as ParameterId, " +
                "p.Id, p.CriticidadeBaixa as LowSeverity, p.CriticidadeMedia as MediumSeverity, p.CriticidadeAlta as HighSeverity, p.PeriodoIndicador as EvaluationPeriod, p.PeriodoHistorico as ComparativePeriod, " +
                "s.Id, s.Nome as Name, s.Ativo as Active " +
                "from Alertas.AlertaGerado ag " +
                "INNER JOIN Alertas.RegraEntidade re on re.Id = ag.RegraAlertaEntidadeId " +
                "LEFT JOIN Alertas.Parametro p on p.Id = re.ParametroId " +
                "INNER JOIN Alertas.Status s on s.Id = ag.AlertaStatusId " +
                "where ag.id = @id",
                map: (alert, entityRule, parameter, status) =>
                {
                    alert.EntityRule = entityRule;
                    alert.EntityRule.Parameter = parameter;
                    alert.Status = status;
                    return alert;
                },
                param: new { id },
                _connector.dbTransaction,
                splitOn: "Id, Id, Id")).FirstOrDefault();
        }

    }
}
