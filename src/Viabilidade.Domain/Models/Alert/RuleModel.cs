using Viabilidade.Domain.Models.Base;

namespace Viabilidade.Domain.Models.Alert
{
    public class RuleModel : BaseModel
    {

        public void NewVersion()
        {
            VersionMajor = Active ?? false ? 1 : 0;
            VersionMinor = Active ?? false ? 0 : 1;
            VersionPatch = 0;
        }

        public void UpdateVersion(RuleModel oldVersion)
        {
            VersionMajor = oldVersion.VersionMajor;
            VersionMinor = oldVersion.VersionMinor;
            VersionPatch = oldVersion.VersionPatch;
            if (
                !Active.Equals(oldVersion.Active) ||
                !IndicatorId.Equals(oldVersion.IndicatorId) ||
                !AlgorithmId.Equals(oldVersion.AlgorithmId) ||
                !OperatorId.Equals(oldVersion.OperatorId) ||
                !Parameter.EvaluationPeriod.Equals(oldVersion.Parameter.EvaluationPeriod) ||
                !Parameter.ComparativePeriod.Equals(oldVersion.Parameter.ComparativePeriod)
            )
            {
                VersionMajor++;
                return;
            }

            if (
                !Name.Equals(oldVersion.Name) ||
                !Parameter.HighSeverity.Equals(oldVersion.Parameter.HighSeverity) ||
                !Parameter.MediumSeverity.Equals(oldVersion.Parameter.MediumSeverity) ||
                !Parameter.LowSeverity.Equals(oldVersion.Parameter.LowSeverity)
            )
            {
                VersionMinor++;
                return;
            }
            if (
                !Description.Equals(oldVersion.Description) ||
                !TagAlerts.Equals(oldVersion.TagAlerts)
            )
            {
               VersionPatch++;
                return;
            }
               
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? AlgorithmId { get; set; }
        public virtual AlgorithmModel Algorithm { get; set; }
        public int? IndicatorId { get; set; }
        public virtual IndicatorModel Indicator { get; set; }
        public int? OperatorId { get; set; }
        public virtual OperatorModel Operator { get; set; }
        public int? IndicatorFilterId { get; set; }
        public virtual IndicatorFilterModel IndicatorFilter { get; set; }
        public int? ParameterId { get; set; }
        public virtual ParameterModel Parameter { get; set; }
        public bool? Active { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public Guid? UserId { get; set; }
        public int? VersionMajor { get; set; }
        public int? VersionMinor { get; set; }
        public int? VersionPatch { get; set; }

        public virtual IEnumerable<FavoriteAlertModel> FavoriteAlerts { get; set; }
        public virtual IEnumerable<TagAlertModel> TagAlerts { get; set; }
        public virtual IEnumerable<EntityRuleModel> EntityRules { get; set; }
    }
}
