using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices
{
    internal class ProgramService : MediaService
    {
        public IObservable<IEnumerable<ProgramModel>> Programs =>
            Observable.Create<IEnumerable<ProgramModel>>(subscriber =>
            {
                JObject json = GetServiceInfo(MediaServicesConstants.Paths.Programs.List);

                if (json == null)
                {
                    subscriber.OnError(new ServiceStatusException("Response is invalid"));
                    return Disposable.Empty;
                }

                IEnumerable<ProgramModel> programs = json.SelectToken(MediaServicesConstants.Json.Value).Select(program =>
                {
                    string status = program.SelectToken(MediaServicesConstants.Json.Status).Value<string>();

                    return new ProgramModel()
                    {
                        Id = program.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                        Name = program.SelectToken(MediaServicesConstants.Json.Name).Value<string>(),
                        Status = MapStatus(status)
                    };
                });

                subscriber.OnNext(programs);
                return Disposable.Empty;
            });
    }
}
