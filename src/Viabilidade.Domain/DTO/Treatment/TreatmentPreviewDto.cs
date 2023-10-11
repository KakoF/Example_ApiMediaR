using Viabilidade.Domain.Models.Alert;

namespace Viabilidade.Domain.DTO.Treatment
{
    public class TreatmentPreviewDto
    {
        public TreatmentModel Treatment { get; set; }
        public RuleModel Rule { get; set; }
    }
}
