﻿using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow
{
    internal class LocatorStepWorkflowModel : IAzureWorkflowModel
    {
        public AssetFileModel AssetFile { get; set; }
        public ChannelModel Channel { get; set; }
        public string Path { get; set; }
        public ProgramModel Program { get; set; }
    }
}
