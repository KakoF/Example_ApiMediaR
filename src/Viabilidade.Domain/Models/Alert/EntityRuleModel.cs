using Viabilidade.Domain.Models.Base;
using Viabilidade.Domain.Models.Org;

namespace Viabilidade.Domain.Models.Alert
{
    public class EntityRuleModel : BaseModel
    {
        public int? RuleId { get; set; }
        public virtual RuleModel Rule { get; set; }
        public int? EntityId { get; set; }
        public EntityModel Entity { get; set; }
        public string SubEntityNumber { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public int? IndicatorFilterId { get; set; }
        public virtual IndicatorFilterModel IndicatorFilter { get; set; }
        public int? ParameterId { get; set; }
        public virtual ParameterModel Parameter { get; set; }
        public bool? Active { get; set; }
        public virtual IEnumerable<AlertModel> Alerts { get; set; }
        public virtual IEnumerable<ChannelModel> Channels { get; set; }
    }
}
