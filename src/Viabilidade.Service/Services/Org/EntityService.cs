using Viabilidade.Domain.Interfaces.Cache;
using Viabilidade.Domain.Interfaces.Repositories.Org;
using Viabilidade.Domain.Interfaces.Services.Org;
using Viabilidade.Domain.Models.Org;

namespace Viabilidade.Service.Services.Org
{
    public class EntityService : IEntityService
    {
        private readonly IEntityRepository _entidadeRepository;
        private readonly IStorageCache _cache;
        public EntityService(IEntityRepository entidadeRepository, IStorageCache cache)
        {
            _entidadeRepository = entidadeRepository;
            _cache = cache;
        }

        public async Task<IEnumerable<EntityModel>> GetAllFilter(int? id, string name, string originalEntityId)
        {
            return await _entidadeRepository.GetAllFilter(id, name, originalEntityId);
        }

        public async Task<IEnumerable<EntityModel>> GetBySegmentSquadAsync(int squadId, int segmentId)
        {
            return await _entidadeRepository.GetBySegmentSquadAsync(squadId, segmentId);
        }

        public async Task<IEnumerable<EntityModel>> GetAsync(bool? active)
        {
            var models = await _cache.GetOrCreateAsync("Entity", () => _entidadeRepository.GetAsync());
            if (active == null)
                return models;
            return models.Where(x => x.Active == active);
        }

        public async Task<EntityModel> GetAsync(int id)
        {
            return await _entidadeRepository.GetAsync(id);
        }

        public async Task<EntityModel> GetByOriginalEntityAsync(int originalEntityId)
        {
            return await _entidadeRepository.GetByOriginalEntityAsync(originalEntityId);
        }
       
    }
}
