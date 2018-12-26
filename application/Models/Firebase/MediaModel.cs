using Google.Cloud.Firestore;

namespace LiteralLifeChurch.LiveStreamingController.Models.Firebase
{
    [FirestoreData]
    internal class MediaModel : IFirebaseModel
    {
        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("url")]
        public string Url { get; set; }
    }
}
