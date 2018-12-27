using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices
{
    internal class AssetFileService : MediaService<AssetFileModel>
    {
        public IObservable<AssetFileStepWorkflowModel> GetIsmFile(string assetId, ChannelModel channel, ProgramModel program)
        {
            return Observable.Create<AssetFileStepWorkflowModel>(subscriber =>
            {
                string path = string.Format(MediaServicesConstants.Paths.AssetFiles.List, assetId);
                JObject json = GetServiceInfo(path);

                if (json == null)
                {
                    subscriber.OnError(new ServiceStatusException("Response is invalid"));
                    return Disposable.Empty;
                }

                AssetFileModel ismFile = json.SelectToken(MediaServicesConstants.Json.Value).Select(file =>
                {
                    return new AssetFileModel
                    {
                        Name = file.SelectToken(MediaServicesConstants.Json.Name).Value<string>()
                    };
                }).Where(asset =>
                {
                    return Regex.IsMatch(asset.Name, MediaServicesConstants.Conventions.AssetFiles.RegexSelector);
                }).First();

                subscriber.OnNext(new AssetFileStepWorkflowModel()
                {
                    AssetFile = ismFile,
                    AssetId = assetId,
                    Channel = channel,
                    Program = program
                });

                subscriber.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}
