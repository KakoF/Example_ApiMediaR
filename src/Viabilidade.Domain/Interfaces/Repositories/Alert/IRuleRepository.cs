using Viabilidade.Domain.DTO.Rule;
using Viabilidade.Domain.Models.Alert;
using Viabilidade.Domain.Models.QueryParams.Rule;

namespace Viabilidade.Domain.Interfaces.Repositories.Alert
{
    public interface IRuleRepository
    {
        Task<RuleModel> CreateAsync(RuleModel entity);
        Task<IEnumerable<RuleModel>> GetAsync();
        Task<IEnumerable<Tuple<RuleGroupDto, int>>> GroupByRuleAsync(RuleQueryParams queryParams);
        Task<IEnumerable<Tuple<RuleGroupDto, int>>> GroupByEntityAsync(RuleQueryParams queryParams);
        Task<RuleModel> GetAsync(int id);
        Task<RuleModel> PreviewAsync(int id);
        Task<RuleModel> UpdateAsync(int id, RuleModel entity);
        Task<bool> DeleteAsync(int id);
    }
}
