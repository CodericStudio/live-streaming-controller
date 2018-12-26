using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;

namespace LiteralLifeChurch.LiveStreamingController.Models.Firebase.Workflow
{
    internal class FirebaseStepWorkflowModel : IFirebaseWorkflowModel
    {
        public ProgramModel Program { get; set; }
    }
}
