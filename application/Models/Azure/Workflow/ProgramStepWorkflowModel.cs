using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow
{
    internal class ProgramStepWorkflowModel
    {
        public string AssetId { get; set; }
        public ProgramModel Program { get; set; }
    }
}
