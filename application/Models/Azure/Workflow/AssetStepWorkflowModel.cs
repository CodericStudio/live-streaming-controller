using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow
{
    internal class AssetStepWorkflowModel : IAzureWorkflowModel
    {
        public string AssetId { get; set; }
        public ChannelModel Channel { get; set; }
    }
}
