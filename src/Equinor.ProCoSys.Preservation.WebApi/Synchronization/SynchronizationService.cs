﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Preservation.Command.MiscCommands.UpdateDateTimeSetting;
using Equinor.ProCoSys.Preservation.Command.TagCommands.AutoTransfer;
using Equinor.ProCoSys.Preservation.Domain;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.SettingAggregate;
using Equinor.ProCoSys.Preservation.Domain.Time;
using Equinor.ProCoSys.Preservation.MainApi.Certificate;
using Equinor.ProCoSys.Preservation.MainApi.Client;
using Equinor.ProCoSys.Preservation.MainApi.Plant;
using Equinor.ProCoSys.Preservation.Query.GetDateTimeSetting;
using Equinor.ProCoSys.Preservation.WebApi.Authentication;
using Equinor.ProCoSys.Preservation.WebApi.Authorizations;
using Equinor.ProCoSys.Preservation.WebApi.Misc;
using Equinor.ProCoSys.Preservation.WebApi.Telemetry;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Preservation.WebApi.Synchronization
{
    public class SynchronizationService : ISynchronizationService
    {
        private readonly Guid _preservationApiOid;
        private readonly ILogger<SynchronizationService> _logger;
        private readonly ITelemetryClient _telemetryClient;
        private readonly IMediator _mediator;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IPlantSetter _plantSetter;
        private readonly ICurrentUserSetter _currentUserSetter;
        private readonly IClaimsTransformation _claimsTransformation;
        private readonly IAuthenticator _authenticator;
        private readonly IPlantCache _plantCache;
        private readonly IOptionsSnapshot<SynchronizationOptions> _options;
        private readonly ICertificateApiService _certificateApiService;

        public SynchronizationService(
            ILogger<SynchronizationService> logger,
            ITelemetryClient telemetryClient,
            IMediator mediator,
            IClaimsProvider claimsProvider,
            IPlantSetter plantSetter,
            ICurrentUserSetter currentUserSetter,
            IClaimsTransformation claimsTransformation,
            IAuthenticator authenticator,
            IPlantCache plantCache,
            IOptionsSnapshot<SynchronizationOptions> options,
            IOptionsSnapshot<AuthenticatorOptions> authenticatorOptions,
            ICertificateApiService certificateApiService)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _mediator = mediator;
            _claimsProvider = claimsProvider;
            _currentUserSetter = currentUserSetter;
            _claimsTransformation = claimsTransformation;
            _plantSetter = plantSetter;
            _authenticator = authenticator;
            _plantCache = plantCache;
            _options = options;
            _certificateApiService = certificateApiService;
            _preservationApiOid = authenticatorOptions.Value.PreservationApiObjectId;
        }

        public async Task Synchronize(CancellationToken cancellationToken)
        {
            _authenticator.AuthenticationType = AuthenticationType.AsApplication;

            _currentUserSetter.SetCurrentUserOid(_preservationApiOid);

            var currentUser = _claimsProvider.GetCurrentUser();
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimsExtensions.Oid, _preservationApiOid.ToString()));
            currentUser.AddIdentity(claimsIdentity);

            foreach (var plant in await _plantCache.GetPlantIdsWithAccessForUserAsync(_preservationApiOid))
            {
                _logger.LogInformation($"Synchronizing plant {plant}...");

                try
                {
                    _plantSetter.SetPlant(plant);
                    await _claimsTransformation.TransformAsync(currentUser);

                    var startTime = TimeService.UtcNow;
                    if (_options.Value.AutoTransferTags)
                    {
                        await AutoTransferTagsAsync(plant);
                    }

                    var endTime = TimeService.UtcNow;

                    _logger.LogInformation($"Plant {plant} synchronized. Duration: {(endTime - startTime).TotalSeconds}s.");
                    _telemetryClient.TrackMetric("Synchronization Time", (endTime - startTime).TotalSeconds, "Plant", plant);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error synchronizing plant {plant}...");
                }
            }
        }

        private async Task AutoTransferTagsAsync(string plant)
        {
            _logger.LogInformation("Autotransfer tags");
            await UpdateLastAcceptedCertificatesRead(plant, Setting.LastAcceptedCertificatesReadingCode);

            var lastAcceptedCertificatesRead = await GetLastAcceptedCertificatesRead(plant);
            if (!lastAcceptedCertificatesRead.HasValue)
            {
                return;
            }

            var acceptedCertificatesSinceLastTransfer = (await _certificateApiService.GetAcceptedCertificatesAsync(plant, lastAcceptedCertificatesRead.Value)).ToList();

            // RFCC must be handled before RFOC, since RFCC is Accepted before RFOC. It is possible the same tag should be transfered 2 steps 
            await AutoTransferTagAffectedByCertificatesAsync(plant, acceptedCertificatesSinceLastTransfer.Where(c => c.CertificateType == "RFCC").ToList());
            await AutoTransferTagAffectedByCertificatesAsync(plant, acceptedCertificatesSinceLastTransfer.Where(c => c.CertificateType == "RFOC").ToList());

            await UpdateLastAcceptedCertificatesRead(plant, Setting.LastAcceptedCertificatesReadCode);
        }

        private async Task UpdateLastAcceptedCertificatesRead(string plant, string code)
        {
            var result = await _mediator.Send(new UpdateDateTimeSettingCommand(code, TimeService.UtcNow));

            if (result.ResultType != ServiceResult.ResultType.Ok)
            {
                _logger.LogWarning(
                    $"Autotransfer tags functions failed. Could not update {code}. ResultType {result.ResultType}");
                _telemetryClient.TrackEvent("Synchronization Status",
                    new Dictionary<string, string>
                    {
                        {"Status", "Failed"},
                        {"Plant", plant},
                        {"Type", "Autotransfer tags"},
                        {"ResultType", result.ResultType.ToString()}
                    });
            }
        }

        private async Task<DateTime?> GetLastAcceptedCertificatesRead(string plant)
        {
            var dateTimeResult =
                await _mediator.Send(new GetDateTimeSettingQuery(Setting.LastAcceptedCertificatesReadCode));
            if (dateTimeResult.ResultType == ServiceResult.ResultType.Ok && dateTimeResult.Data.HasValue)
            {
                return dateTimeResult.Data.Value;
            }

            if (dateTimeResult.ResultType == ServiceResult.ResultType.NotFound
                || (dateTimeResult.ResultType == ServiceResult.ResultType.Ok && !dateTimeResult.Data.HasValue))
            {
                return TimeService.UtcNow;
            }

            _logger.LogWarning(
                $"Autotransfer tags functions failed. Could not get {Setting.LastAcceptedCertificatesReadCode}. ResultType {dateTimeResult.ResultType}");
            _telemetryClient.TrackEvent("Synchronization Status",
                new Dictionary<string, string>
                {
                    {"Status", "Failed"},
                    {"Plant", plant},
                    {"Type", "Autotransfer tags"},
                    {"ResultType", dateTimeResult.ResultType.ToString()}
                });
            return null;
        }

        private async Task AutoTransferTagAffectedByCertificatesAsync(string plant, List<PCSCertificateModel> certificates)
        {
            foreach (var certificate in certificates)
            {
                var result = await _mediator.Send(new AutoTransferCommand(certificate.ProjectName, certificate.CertificateNo, certificate.CertificateType));

                if (result.ResultType == ServiceResult.ResultType.Ok)
                {
                    _logger.LogInformation("Autotransfer tags complete.");
                    _telemetryClient.TrackEvent("Synchronization Status",
                        new Dictionary<string, string>
                        {
                            {"Status", "Succeeded"}, 
                            {"Plant", plant}, 
                            {"Type", "Autotransfer tags"}, 
                            {"ProjectName", certificate.ProjectName}, 
                            {"CertificateNo", certificate.CertificateNo}, 
                            {"CertificateType", certificate.CertificateType}
                        });
                }
                else
                {
                    _logger.LogWarning($"Autotransfer tags functions failed.");
                    _telemetryClient.TrackEvent("Synchronization Status",
                        new Dictionary<string, string>
                        {
                            {"Status", "Failed"}, 
                            {"Plant", plant}, 
                            {"Type", "Autotransfer tags"}, 
                            {"ProjectName", certificate.ProjectName}, 
                            {"CertificateNo", certificate.CertificateNo}, 
                            {"CertificateType", certificate.CertificateType}
                        });
                }
            }
        }
    }
}
