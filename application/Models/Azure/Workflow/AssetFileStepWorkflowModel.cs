using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow
{
    internal class AssetFileStepWorkflowModel : IAzureWorkflowModel
    {
        public AssetFileModel AssetFile { get; set; }
        public string AssetId { get; set; }
        public ChannelModel Channel { get; set; }
        public ProgramModel Program { get; set; }
    }
}
