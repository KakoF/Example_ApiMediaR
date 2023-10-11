using Viabilidade.Domain.Models.Org;

namespace Viabilidade.Domain.Interfaces.Repositories.Org
{
    public interface IEntityRepository : IBaseRepository<EntityModel>
    {
        Task<EntityModel> GetByOriginalEntityAsync(int originalEntityId);
        Task<IEnumerable<EntityModel>> GetAllFilter(int? id, string name, string originalEntityId);
        Task<IEnumerable<EntityModel>> GetBySegmentSquadAsync(int squadId, int segmentId);
    }
}