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
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.GroundStation.SendToMqtt {

    [ExportMetadata("Name", "Send to MQTT")]
    [ExportMetadata("Description", "Sends a free form message to a MQTT broker")]
    [ExportMetadata("Icon", "Mqtt_SVG")]
    [ExportMetadata("Category", "Ground Station")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SendToMqtt : SequenceItem, IValidatable {
        private string topic;
        private string payload = string.Empty;

        [ImportingConstructor]
        public SendToMqtt() {
            MqttBrokerHost = Properties.Settings.Default.MqttBrokerHost;
            MqttBrokerPort = Properties.Settings.Default.MqttBrokerPort;
            MqttBrokerUseTls = Properties.Settings.Default.MqttBrokerUseTls;
            MqttUsername = Security.Decrypt(Properties.Settings.Default.MqttUsername);
            MqttPassword = Security.Decrypt(Properties.Settings.Default.MqttPassword);
            MqttClientId = Properties.Settings.Default.MqttClientId;
            Topic = Properties.Settings.Default.MqttDefaultTopic;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        public SendToMqtt(SendToMqtt copyMe) : this() {
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

        [JsonProperty]
        public string Payload {
            get => payload;
            set {
                payload = value;
                RaisePropertyChanged();
            }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
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

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(Topic)
                .WithPayload(Payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            var discopts = new MqttClientDisconnectOptions();

            Logger.Debug("SendToMqtt: Pushing message");

            try {
                await mqttClient.ConnectAsync(opts, ct);
                await mqttClient.PublishAsync(message, ct);
                await mqttClient.DisconnectAsync(discopts, ct);
                mqttClient.Dispose();
            } catch (Exception ex) {
                Logger.Error($"Error sending to MQTT broker: {ex.Message}");
                throw ex;
            }
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
            return new SendToMqtt() {
                Icon = Icon,
                Name = Name,
                Topic = Topic,
                Payload = Payload,
                Category = Category,
                Description = Description,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(SendToMqtt)}";
        }

        private string MqttBrokerHost { get; set; }
        private ushort MqttBrokerPort { get; set; }
        private bool MqttBrokerUseTls { get; set; }
        private string MqttUsername { get; set; }
        private string MqttPassword { get; set; }
        private string MqttClientId { get; set; }

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