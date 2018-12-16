using Firebase.Database;
using LiteralLifeChurch.LiveStreamingController.Repositories.Firebase;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingController.Services.Firebase
{
    internal class FirebaseService
    {

        public IObservable<bool> PublishUrls()
        {
            return Observable.FromAsync(async () => await Client.Child("media").PutAsync("test"))
                .Select(outcome =>
                {
                    return true;
                });
        }

        private FirebaseClient Client => new FirebaseClient(FirebaseConfigurationRepository.AppUrl, new FirebaseOptions
        {
            AuthTokenAsyncFactory = () => Task.FromResult(FirebaseConfigurationRepository.AppSecret)
        });
    }
}
