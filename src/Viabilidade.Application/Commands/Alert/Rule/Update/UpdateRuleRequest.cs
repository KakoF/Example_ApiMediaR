using MediatR;
using System.Text.Json.Serialization;
using Viabilidade.Domain.Models.Alert;

namespace Viabilidade.Application.Commands.Alert.Rule.Update
{
    public class UpdateRuleRequest : IRequest<RuleModel>
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AlgorithmId { get; set; }
        public int IndicatorId { get; set; }
        public int OperatorId { get; set; }
        public bool Active { get; set; }
        public ParameterRequest Parameter { get; set; }
        public IEnumerable<UpdateTagsRequest> Tags { get; set; }
        public IEnumerable<EntityRuleRequest> UpdateEntityRules { get; set; }
        public IEnumerable<EntityRuleRequest> CreateEntityRules { get; set; }

        public UpdateRuleRequest(int id, string name, string description, int algorithmId, int indicatorId, int operatorId, bool active, ParameterRequest parameter, IEnumerable<UpdateTagsRequest> tags, IEnumerable<EntityRuleRequest> updateEntityRules, IEnumerable<EntityRuleRequest> createEntityRules)
        {
            Id = id;
            Name = name;
            Description = description;
            AlgorithmId = algorithmId;
            IndicatorId = indicatorId;
            OperatorId = operatorId;
            Active = active;
            Parameter = parameter;
            Tags = tags;
            UpdateEntityRules = updateEntityRules;
            CreateEntityRules = createEntityRules;
        }
    }

    public class UpdateTagsRequest
    {
        public int Id { get; set; }
    }

    public class ParameterRequest
    {
        public decimal LowSeverity { get; set; }
        public decimal MediumSeverity { get; set; }
        public decimal HighSeverity { get; set; }
        public decimal EvaluationPeriod { get; set; }
        public decimal? ComparativePeriod { get; set; }
    }
   
    public class EntityRuleRequest
    {
        public int EntityId { get; set; }
        public bool Active { get; set; }
        public int? ChannelId { get; set; }
        public ParameterRequest Parameter { get; set; }
    }

}
