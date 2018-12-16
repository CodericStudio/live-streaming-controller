using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow
{
    internal class AccessPolicyStepWorkflowModel : IWorkflowModel
    {
        public string AccessPolicyId { get; set; }
        public string AssetId { get; set; }
        public ProgramModel Program { get; set; }
    }
}
