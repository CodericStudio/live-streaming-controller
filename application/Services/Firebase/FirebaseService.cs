using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1Beta1;
using Grpc.Auth;
using Grpc.Core;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Models.Firebase;
using LiteralLifeChurch.LiveStreamingController.Models.Firebase.Workflow;
using LiteralLifeChurch.LiveStreamingController.Repositories.Firebase;
using LiteralLifeChurch.LiveStreamingController.Services.Azure;
using System;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace LiteralLifeChurch.LiveStreamingController.Services.Firebase
{
    internal class FirebaseService
    {
        public IObservable<bool> DeleteAll => Observable.FromAsync(async () =>
        {
            GoogleCredential credentials = GoogleCredential.FromJson(FirebaseConfigurationRepository.CredentialString);
            Channel channel = new Channel(FirestoreClient.DefaultEndpoint.Host, FirestoreClient.DefaultEndpoint.Port, credentials.ToChannelCredentials());
            FirestoreClient client = FirestoreClient.Create(channel);
            FirestoreDb database = FirestoreDb.Create(FirebaseConfigurationRepository.ProjectId, null, client);

            CollectionReference collection = database.Collection(FirebaseConstants.CollectionName);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                await document.Reference.DeleteAsync();
            }

            await channel.ShutdownAsync();
            return true;
        });

        public IObservable<FirebaseStepWorkflowModel> PublishUrl(string channelName, string locatorUrl, string ismFileName, ProgramModel program)
        {
            return Observable.FromAsync(async () =>
            {
                GoogleCredential credentials = GoogleCredential.FromJson(FirebaseConfigurationRepository.CredentialString);
                Channel channel = new Channel(FirestoreClient.DefaultEndpoint.Host, FirestoreClient.DefaultEndpoint.Port, credentials.ToChannelCredentials());
                FirestoreClient client = FirestoreClient.Create(channel);
                FirestoreDb database = FirestoreDb.Create(FirebaseConfigurationRepository.ProjectId, null, client);

                CollectionReference collection = database.Collection(FirebaseConstants.CollectionName);

                string url = string.Format(MediaServicesConstants.Paths.AssetFiles.ManifestPath, locatorUrl, ismFileName);
                Regex regex = new Regex(MediaServicesConstants.Conventions.Locators.RegexProtocol);
                string httpsUrl = regex.Replace(url, MediaServicesConstants.Conventions.Locators.ProtocolReplacement);

                await collection.AddAsync(new MediaModel()
                {
                    Name = channelName,
                    Url = httpsUrl
                });

                await channel.ShutdownAsync();
                return new FirebaseStepWorkflowModel()
                {
                    Program = program
                };
            });
        }
    }
}
