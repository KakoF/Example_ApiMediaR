using Dapper;
using Viabilidade.Domain.Interfaces.Repositories.Org;
using Viabilidade.Domain.Models.Org;
using Viabilidade.Infrastructure.Interfaces.DataConnector;

namespace Viabilidade.Infrastructure.Repositories.Org
{
    public class EntityRepository : BaseRepository<EntityModel>, IEntityRepository
    {
        protected override string _database => "Org.Entidade";
        protected override string _selectCollumns => "Id, Nome as Name, Ativo as Active, EntidadeIdOriginal as OriginalEntityId, SegmentoId as SegmentId";
        private readonly IDbConnector _connector;

        public EntityRepository(IDbConnector connector) : base(connector)
        {
            _connector = connector;
        }

        public async Task<IEnumerable<EntityModel>> GetAllFilter(int? id, string name, string originalEntityId)
        {
            return await _connector.dbConnection.QueryAsync<EntityModel>($"Select TOP 50 {_selectCollumns} from {_database} Where Ativo = 1 " +
                $"{(!string.IsNullOrEmpty(name) ? "AND Nome like @Nome " : "")}" +
                $"{(id.HasValue && id > 0 ? "AND Id = @id " : "")}" +
                $"{(!string.IsNullOrEmpty(originalEntityId) ? "AND CAST(EntidadeIdOriginal AS VARCHAR(MAX)) like @EntidadeIdOriginal " : "")} " +
                $"ORDER BY Nome ", new { id, Nome = $"%{name}%", EntidadeIdOriginal = $"%{originalEntityId}%" }, _connector.dbTransaction);
        }

        public async Task<IEnumerable<EntityModel>> GetBySegmentSquadAsync(int squadId, int segmentId)
        {
            return await _connector.dbConnection.QueryAsync<EntityModel>($"SELECT ent.Id, ent.Nome as Name, ent.EntidadeIdOriginal as OriginalEntityId, ent.Ativo as Active, ent.SegmentoId as SegmentId " +
                            $"FROM {_database} ent " +
                            "INNER JOIN Org.R_SquadEntidade rel ON rel.EntidadeId = ent.Id " +
                            "WHERE ent.Ativo = 1 AND rel.SquadId = @squadId and ent.SegmentoId = @segmentId ", new { squadId, segmentId }, _connector.dbTransaction);
        }



        public async Task<EntityModel> GetByOriginalEntityAsync(int originalEntityId)
        {
            var channels = new List<ChannelModel>();
            return (await _connector.dbConnection.QueryAsync<EntityModel, ChannelModel, EntityModel>(
                "SELECT ent.Id, ent.Nome as Name, ent.EntidadeIdOriginal as OriginalEntityId, ent.Ativo as Active, ent.SegmentoId as SegmentId, " +
                "s.Id, s.Nome As Name, s.Ativo As Active " +
                $"FROM {_database} ent " +
                "left JOIN Org.R_SquadEntidade rel ON rel.EntidadeId = ent.Id " +
                "left join Org.Squad s on s.Id = rel.SquadId " +
                "where ent.EntidadeIdOriginal = @originalEntityId",
                map: (entity, channel) =>
                {
                    if (channel != null)
                    {
                        if (channels.FirstOrDefault(x => x.Id == channel.Id) == null)
                            channels.Add(channel);
                    }
                    entity.Channels = channels;
                    return entity;
                },
                param: new { originalEntityId },
                _connector.dbTransaction,
                splitOn: "Id")).FirstOrDefault();
        }
    }
}
