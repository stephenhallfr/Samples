﻿using System;

using Microsoft.Azure.EventHubs;

using Microsoft.Azure.EventHubs.Processor;

using System.Threading.Tasks;

using System.Collections.Generic;

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;

namespace TestHostEPH
{

    public class SimpleEventProcessor : IEventProcessor
    {
 
        Microsoft.ApplicationInsights.TelemetryClient telemetry = null;
        private static IConfigurationRoot Configuration { get; set; }

        public SimpleEventProcessor()
        {

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.development.json", false, true);
            Configuration = builder.Build();

            var appinsightKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            var config = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(appinsightKey);
            telemetry = new Microsoft.ApplicationInsights.TelemetryClient(config);

            telemetry.TrackTrace("SimpleEventProcessor initialized...", SeverityLevel.Information);
        }


        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {

            try
            {
                telemetry.TrackTrace($"CloseAsync. Partition '{context.PartitionId}', Reason: '{reason}'.", SeverityLevel.Information);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw ex;
            }
            return Task.CompletedTask;

        }
            



        public Task OpenAsync(PartitionContext context)
        {

            try
            {
                telemetry.TrackTrace($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'", SeverityLevel.Information);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw ex;
            }

            return Task.CompletedTask;

        }



        public Task ProcessErrorAsync(PartitionContext context, Exception error)

        {

            try
            {
                telemetry.TrackTrace($"Error on Partition: {context.PartitionId}, Error: {error.Message}", SeverityLevel.Information);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw ex;
            }

            return Task.CompletedTask;

        }



        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            try
            {
                foreach (var eventData in messages)
                {

                    var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    var tt = new TraceTelemetry();

                    telemetry.TrackTrace(
                        $"EPH ProcessEventsAsync (Offset={eventData.SystemProperties.Offset}, " +
                        $"SequenceNumber={eventData.SystemProperties.SequenceNumber}, " +
                        $"EnqueueTimeUtc={eventData.SystemProperties.EnqueuedTimeUtc}, " +
                        $"EnqueueTime-Now={System.DateTime.UtcNow.Subtract(eventData.SystemProperties.EnqueuedTimeUtc).TotalMilliseconds}, " +
                        $"PartitionId={context.PartitionId})", SeverityLevel.Verbose);

                }
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                throw ex;
            }
            return context.CheckpointAsync();

        }

    }

}