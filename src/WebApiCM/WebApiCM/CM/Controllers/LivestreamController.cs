using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using Microsoft.WindowsAzure.MediaServices.Client.Live;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using WebApiCM.Models;

namespace WebApiCM.CM.Controllers{
    [RoutePrefix("api/cm/livestream")]  
    public class LivestreamController : ApiController 
    {

        //[SwaggerOperation("GetAll")]
        //public IEnumerable<Banner> Get() {
        //    ApplicationDbContext dbContext = new ApplicationDbContext();
        //    return dbContext.Banners.ToArray();
        //}

        private const string ChannelName = "channel001";
        private const string AssetName = "asset006";
        private const string ProgramName = "program001";

        // Read values from the App.config file.
        private static readonly string _mediaServicesAccountName = ConfigurationManager.AppSettings["MediaServicesAccountName"];
        private static readonly string _mediaServicesAccountKey = ConfigurationManager.AppSettings["MediaServicesAccountKey"];

        // Field for service context.
        private static CloudMediaContext _context = null;
        private static MediaServicesCredentials _cachedCredentials = null;

        // GET api/cm/livestream
        [SwaggerOperation("Get RTMP")]
        [Route("")]
        public IHttpActionResult GetRTMP() {

            // Create and cache the Media Services credentials in a static class variable.
            _cachedCredentials = new MediaServicesCredentials(_mediaServicesAccountName, _mediaServicesAccountKey);
            // Used the cached credentials to create CloudMediaContext.
            _context = new CloudMediaContext(_cachedCredentials);

            var defaultStreamingEndpoint = _context.StreamingEndpoints.Where(s => s.Name.Contains("default")).FirstOrDefault();
            defaultStreamingEndpoint.Start();

            IChannel channel = CreateAndStartChannel();

            // The channel's input endpoint:
            string ingestUrl = channel.Input.Endpoints.FirstOrDefault().Url.ToString();

            // Use the previewEndpoint to preview and verify that the input from the encoder is actually reaching the Channel. 
            string previewEndpoint = channel.Preview.Endpoints.FirstOrDefault().Url.ToString();

            // Once you previewed your stream and verified that it is flowing into your Channel, you can create an event by creating an Asset, Program, and Streaming Locator. 
            IAsset asset = CreateAndConfigureAsset();

            IProgram program = CreateAndStartProgram(channel, asset);

            ILocator locator = CreateLocatorForAsset(program.Asset, program.ArchiveWindowLength);

            // Once you are done streaming, clean up your resources.           

            string ingestPreview = string.Format("Ingest RTMP: {0}, Preview: {1}", ingestUrl, previewEndpoint);

            return Ok(ingestPreview);
        }

        public static IChannel CreateAndStartChannel() {
            var channelInput = CreateChannelInput();
            var channePreview = CreateChannelPreview();
            //var channelEncoding = CreateChannelEncoding();

            ChannelCreationOptions options = new ChannelCreationOptions {
                //EncodingType = ChannelEncodingType.Standard,                
                //EncodingType = ChannelEncodingType.None,
                Name = ChannelName,
                Input = channelInput,
                Preview = channePreview,
                //Encoding = channelEncoding
            };

            //Log("Creating channel");
            IOperation channelCreateOperation = _context.Channels.SendCreateOperation(options);
            string channelId = TrackOperation(channelCreateOperation, "Channel create");
            //Console.WriteLine(ChannelEncodingType.Standard.ToString());

            IChannel channel = _context.Channels.Where(c => c.Id == channelId).FirstOrDefault();

            //Log("Starting channel");
            var channelStartOperation = channel.SendStartOperation();
            TrackOperation(channelStartOperation, "Channel start");

            return channel;
        }

        /// <summary>
        /// Create channel input, used in channel creation options. 
        /// </summary>
        /// <returns></returns>
        private static ChannelInput CreateChannelInput() {
            return new ChannelInput {
                StreamingProtocol = StreamingProtocol.RTMP,
                AccessControl = new ChannelAccessControl {
                    IPAllowList = new List<IPRange>
                    {
                        new IPRange
                        {
                            Name = "TestChannelInput001",
                            Address = IPAddress.Parse("0.0.0.0"),
                            SubnetPrefixLength = 0
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Create channel preview, used in channel creation options. 
        /// </summary>
        /// <returns></returns>
        private static ChannelPreview CreateChannelPreview() {
            return new ChannelPreview {
                AccessControl = new ChannelAccessControl {
                    IPAllowList = new List<IPRange>
                    {
                        new IPRange
                        {
                            Name = "TestChannelPreview001",
                            Address = IPAddress.Parse("0.0.0.0"),
                            SubnetPrefixLength = 0
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Create channel encoding, used in channel creation options. 
        /// </summary>
        /// <returns></returns>
        private static ChannelEncoding CreateChannelEncoding() {
            return new ChannelEncoding {
                SystemPreset = "Default720p",
                //IgnoreCea708ClosedCaptions = false,
                //AdMarkerSource = AdMarkerSource.Api,
                // You can only set audio if streaming protocol is set to StreamingProtocol.RTPMPEG2TS.
                //AudioStreams = new List<AudioStream> { new AudioStream { Index = 103, Language = "eng" } }.AsReadOnly()
            };
        }

        /// <summary>
        /// Create an asset and configure asset delivery policies.
        /// </summary>
        /// <returns></returns>
        public static IAsset CreateAndConfigureAsset() {
            IAsset asset = _context.Assets.Create(AssetName, AssetCreationOptions.None);

            IAssetDeliveryPolicy policy =
                _context.AssetDeliveryPolicies.Create("Clear Policy",
                AssetDeliveryPolicyType.NoDynamicEncryption,
                AssetDeliveryProtocol.HLS | AssetDeliveryProtocol.SmoothStreaming | AssetDeliveryProtocol.Dash, null);
            asset.DeliveryPolicies.Add(policy);

            return asset;
        }

        /// <summary>
        /// Create a Program on the Channel. You can have multiple Programs that overlap or are sequential;
        /// however each Program must have a unique name within your Media Services account.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static IProgram CreateAndStartProgram(IChannel channel, IAsset asset) {
            IProgram program = channel.Programs.Create(ProgramName, TimeSpan.FromHours(8), asset.Id);
            //Log("Program created", program.Id);

            //Log("Starting program");
            var programStartOperation = program.SendStartOperation();
            TrackOperation(programStartOperation, "Program start");

            return program;
        }

        /// <summary>
        /// Create locators in order to be able to publish and stream the video.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="ArchiveWindowLength"></param>
        /// <returns></returns>
        public static ILocator CreateLocatorForAsset(IAsset asset, TimeSpan ArchiveWindowLength) {
            var locator = _context.Locators.CreateLocator
                (
                    LocatorType.OnDemandOrigin,
                    asset,
                    _context.AccessPolicies.Create
                        (
                            "Live Stream Policy",
                            ArchiveWindowLength,
                            AccessPermissions.Read
                        )
                );

            return locator;
        }

        /// <summary>
        /// Clean up resources associated with the channel.
        /// </summary>
        /// <param name="channel"></param>
        public static void Cleanup(IChannel channel) {
            IAsset asset;
            if (channel != null) {
                foreach (var program in channel.Programs) {
                    asset = _context.Assets.Where(se => se.Id == program.AssetId)
                                            .FirstOrDefault();

                    Log("Stopping program");
                    var programStopOperation = program.SendStopOperation();
                    TrackOperation(programStopOperation, "Program stop");

                    program.Delete();

                    if (asset != null) {
                        Log("Deleting locators");
                        foreach (var l in asset.Locators)
                            l.Delete();

                        Log("Deleting asset");
                        asset.Delete();
                    }
                }

                Log("Stopping channel");
                var channelStopOperation = channel.SendStopOperation();
                TrackOperation(channelStopOperation, "Channel stop");

                Log("Deleting channel");
                var channelDeleteOperation = channel.SendDeleteOperation();
                TrackOperation(channelDeleteOperation, "Channel delete");
            }
        }

        public static void CleanupWithoutAssets(IChannel channel) {
            IAsset asset;
            if (channel != null) {
                foreach (var program in channel.Programs) {
                    asset = _context.Assets.Where(se => se.Id == program.AssetId)
                                            .FirstOrDefault();

                    Log("Stopping program");
                    var programStopOperation = program.SendStopOperation();
                    TrackOperation(programStopOperation, "Program stop");

                    program.Delete();

                    //if (asset != null)
                    //{
                    //    Log("Deleting locators");
                    //    foreach (var l in asset.Locators)
                    //        l.Delete();

                    //    Log("Deleting asset");
                    //    asset.Delete();
                    //}
                }

                Log("Stopping channel");
                var channelStopOperation = channel.SendStopOperation();
                TrackOperation(channelStopOperation, "Channel stop");

                Log("Deleting channel");
                var channelDeleteOperation = channel.SendDeleteOperation();
                TrackOperation(channelDeleteOperation, "Channel delete");
            }
        }

        /// <summary>
        /// Track long running operations.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static string TrackOperation(IOperation operation, string description) {
            string entityId = null;
            bool isCompleted = false;

            //Log("starting to track ", null, operation.Id);
            while (isCompleted == false) {
                operation = _context.Operations.GetOperation(operation.Id);
                isCompleted = IsCompleted(operation, out entityId);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(30));
            }
            // If we got here, the operation succeeded.
            //Log(description + " in completed", operation.TargetEntityId, operation.Id);

            return entityId;
        }

        /// <summary> 
        /// Checks if the operation has been completed. 
        /// If the operation succeeded, the created entity Id is returned in the out parameter.
        /// </summary> 
        /// <param name="operationId">The operation Id.</param> 
        /// <param name="channel">
        /// If the operation succeeded, 
        /// the entity Id associated with the sucessful operation is returned in the out parameter.</param>
        /// <returns>Returns false if the operation is still in progress; otherwise, true.</returns> 
        private static bool IsCompleted(IOperation operation, out string entityId) {

            bool completed = false;

            entityId = null;

            switch (operation.State) {
                case OperationState.Failed:
                    // Handle the failure. 
                    // For example, throw an exception. 
                    // Use the following information in the exception: operationId, operation.ErrorMessage.
                    //Log("operation failed", operation.TargetEntityId, operation.Id);
                    break;
                case OperationState.Succeeded:
                    completed = true;
                    entityId = operation.TargetEntityId;
                    break;
                case OperationState.InProgress:
                    completed = false;
                    //Log("operation in progress", operation.TargetEntityId, operation.Id);
                    break;
            }
            return completed;
        }


        private static void Log(string action, string entityId = null, string operationId = null) {
            Console.WriteLine(
                "{0,-21}{1,-51}{2,-51}{3,-51}",
                DateTime.Now.ToString("yyyy'-'MM'-'dd HH':'mm':'ss"),
                action,
                entityId ?? string.Empty,
                operationId ?? string.Empty);
        }
                 
    }
}