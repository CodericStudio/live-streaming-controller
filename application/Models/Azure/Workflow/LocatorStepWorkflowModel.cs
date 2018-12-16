﻿using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow
{
    internal class LocatorStepWorkflowModel : IWorkflowModel
    {
        public string Path { get; set; }
        public ProgramModel Program { get; set; }
    }
}
