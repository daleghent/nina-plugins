#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using MQTTnet;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Trigger;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.GroundStation.FailuresToMqttTrigger {

    [ExportMetadata("Name", "Failures to MQTT")]
    [ExportMetadata("Description", "Sends a JSON object to an MQTT broker and topic when a sequence instruction fails")]
    [ExportMetadata("Icon", "Mqtt_SVG")]
    [ExportMetadata("Category", "Ground Station")]
    [Export(typeof(ISequenceTrigger))]
    [JsonObject(MemberSerialization.OptIn)]
    public class FailuresToMqttTrigger : SequenceTrigger, IValidatable {
        private ISequenceItem previousItem;
        private string topic;

        [ImportingConstructor]
        public FailuresToMqttTrigger() {
            MqttBrokerHost = Properties.Settings.Default.MqttBrokerHost;
            MqttBrokerPort = Properties.Settings.Default.MqttBrokerPort;
            MqttBrokerUseTls = Properties.Settings.Default.MqttBrokerUseTls;
            MqttUsername = Security.Decrypt(Properties.Settings.Default.MqttUsername);
            MqttPassword = Security.Decrypt(Properties.Settings.Default.MqttPassword);
            MqttClientId = Properties.Settings.Default.MqttClientId;
            Topic = Properties.Settings.Default.MqttDefaultTopic;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        public FailuresToMqttTrigger(FailuresToMqttTrigger copyMe) : this() {
            CopyMetaData(copyMe);
        }

        [JsonProperty]
        public string Topic {
            get => topic;
            set {
                topic = value;
                RaisePropertyChanged();
            }
        }

        public override async Task Execute(ISequenceContainer context, IProgress<ApplicationStatus> progress, CancellationToken ct) {
            var itemInfo = new PreviousItem {
                name = previousItem.Name,
                description = previousItem.Description,
                attempts = previousItem.Attempts,
                error_list = new List<ErrorItems>()
            };

            if (PreviousItemIssues.Count > 0) {
                foreach (var e in PreviousItemIssues) {
                    itemInfo.error_list.Add(new ErrorItems { reason = e, });
                }
            }

            Logger.Trace(JsonConvert.SerializeObject(itemInfo));

            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(MqttClientId)
                .WithTcpServer(MqttBrokerHost, MqttBrokerPort)
                .WithCleanSession();

            if (!string.IsNullOrEmpty(MqttUsername) && !string.IsNullOrWhiteSpace(MqttUsername)) {
                options.WithCredentials(MqttUsername, MqttPassword);
            }

            if (MqttBrokerUseTls) {
                options.WithTls();
            }

            var opts = options.Build();

            var payload = new MqttApplicationMessageBuilder()
                .WithTopic(Topic)
                .WithPayload(JsonConvert.SerializeObject(itemInfo))
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            var discopts = new MqttClientDisconnectOptions();

            Logger.Debug("MqttTrigger: Pushing message");

            try {
                await mqttClient.ConnectAsync(opts, ct);
                await mqttClient.PublishAsync(payload, ct);
                await mqttClient.DisconnectAsync(discopts, ct);
                mqttClient.Dispose();
            } catch (Exception ex) {
                Logger.Error($"Error sending to MQTT broker: {ex.Message}");
                throw;
            }
        }

        public override bool ShouldTrigger(ISequenceItem previousItem, ISequenceItem nextItem) {
            bool shouldTrigger = false;

            if (previousItem == null) {
                Logger.Debug("MqttTrigger: Previous item is null. Asserting false");
                return shouldTrigger; ;
            }

            this.previousItem = previousItem;

            if (this.previousItem.Status == SequenceEntityStatus.FAILED && !this.previousItem.Name.Contains("MQTT")) {
                Logger.Debug($"MqttTrigger: Previous item \"{this.previousItem.Name}\" failed. Asserting true");
                shouldTrigger = true;

                if (this.previousItem is IValidatable validatableItem && validatableItem.Issues.Count > 0) {
                    PreviousItemIssues = validatableItem.Issues;
                    Logger.Debug($"MqttTrigger: Previous item \"{this.previousItem.Name}\" had {PreviousItemIssues.Count} issues: {string.Join(", ", PreviousItemIssues)}");
                }
            } else {
                Logger.Debug($"MqttTrigger: Previous item \"{this.previousItem.Name}\" did not fail. Asserting false");
            }

            return shouldTrigger;
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            if (string.IsNullOrEmpty(MqttBrokerHost) || string.IsNullOrWhiteSpace(MqttBrokerHost)) {
                i.Add("MQTT broker hostname or IP not configured");
            }

            if (string.IsNullOrEmpty(MqttClientId) || string.IsNullOrWhiteSpace(MqttClientId)) {
                i.Add("MQTT client ID is invalid!");
            }

            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged("Issues");
            }

            return i.Count == 0;
        }

        public override object Clone() {
            return new FailuresToMqttTrigger() {
                Icon = Icon,
                Name = Name,
                Topic = Topic,
                Category = Category,
                Description = Description,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(FailuresToMqttTrigger)}";
        }

        private IList<string> PreviousItemIssues { get; set; } = new List<string>();
        private string MqttBrokerHost { get; set; }
        private ushort MqttBrokerPort { get; set; }
        private bool MqttBrokerUseTls { get; set; }
        private string MqttUsername { get; set; }
        private string MqttPassword { get; set; }
        private string MqttClientId { get; set; }

        private class PreviousItem {
            public string name { get; set; }
            public string description { get; set; }
            public int attempts { get; set; }
            public List<ErrorItems> error_list { get; set; }
        }

        public class ErrorItems {
            public string reason { get; set; }
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "MqttBrokerHost":
                    MqttBrokerHost = Properties.Settings.Default.MqttBrokerHost;
                    break;
                case "MqttBrokerPort":
                    MqttBrokerPort = Properties.Settings.Default.MqttBrokerPort;
                    break;
                case "MqttBrokerUseTls":
                    MqttBrokerUseTls = Properties.Settings.Default.MqttBrokerUseTls;
                    break;
                case "MqttUsername":
                    MqttUsername = Security.Decrypt(Properties.Settings.Default.MqttUsername);
                    break;
                case "MqttPassword":
                    MqttPassword = Security.Decrypt(Properties.Settings.Default.MqttPassword);
                    break;
                case "MqttClientId":
                    MqttClientId = Properties.Settings.Default.MqttClientId;
                    break;
            }
        }
    }
}