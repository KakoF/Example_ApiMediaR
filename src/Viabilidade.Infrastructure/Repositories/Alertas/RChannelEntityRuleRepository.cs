using Dapper;
using Viabilidade.Domain.Interfaces.Repositories.Alert;
using Viabilidade.Domain.Models.Alert;
using Viabilidade.Infrastructure.Interfaces.DataConnector;

namespace Viabilidade.Infrastructure.Repositories.Alertas
{
    public class RChannelEntityRuleRepository : IRChannelEntityRuleRepository
    {
        private readonly IDbConnector _connector;
        public RChannelEntityRuleRepository(IDbConnector connector)
        {
            _connector = connector;
        }
        public async Task<RChannelEntityRuleModel> CreateAsync(RChannelEntityRuleModel entity)
        {
            entity.Id = await _connector.dbConnection.QuerySingleAsync<int>("INSERT INTO Alertas.R_RegraEntidadeCanal" +
                "(RegraEntidadeId, CanalId) OUTPUT Inserted.Id " +
                "VALUES" +
                "(@RegraEntidadeId, @CanalId)"
                , new { RegraEntidadeId = entity.EntityRuleId, CanalId = entity.ChannelId }, _connector.dbTransaction);
            return entity;
        }

        public async Task<bool> DeleteByEntityRuleAsync(IEnumerable<int> entityRuleIds)
        {
            var delete = await _connector.dbConnection.ExecuteAsync("DELETE FROM Alertas.R_RegraEntidadeCanal where RegraEntidadeId in @entityRuleIds", new { entityRuleIds }, _connector.dbTransaction);
            return Convert.ToBoolean(delete);
        }
    }
}
