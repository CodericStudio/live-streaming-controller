using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow
{
    internal class ProgramStepWorkflowModel : IAzureWorkflowModel
    {
        public string AssetId { get; set; }
        public ChannelModel Channel { get; set; }
        public ProgramModel Program { get; set; }
    }
}
