#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using DaleGhent.NINA.PlaneWaveTools.Utility;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.WaitForCooledMirror {

    [ExportMetadata("Name", "Wait For Cooled Mirror")]
    [ExportMetadata("Description", "When reached, this instruction does not exit unil the primary mirror's temperature is within the specified difference from ambient")]
    [ExportMetadata("Icon", "ThermometerSVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class WaitForCooledMirror : SequenceItem, IValidatable, INotifyPropertyChanged {
        private double maxAmbientDeltaT = 3d;
        private AmbientTempSourceEnum ambientTempSource = AmbientTempSourceEnum.DeltaT;
        private readonly string pwi3UrlBase = "/";
        private IFocuserMediator focuserMediator;
        private IWeatherDataMediator weatherDataMediator;

        [ImportingConstructor]
        public WaitForCooledMirror(IFocuserMediator focuserMediator, IWeatherDataMediator weatherDataMediator) {
            Pwi3IpAddress = Properties.Settings.Default.Pwi3IpAddress;
            Pwi3Port = Properties.Settings.Default.Pwi3Port;
            Pwi3ClientId = Properties.Settings.Default.Pwi3ClientId;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;

            this.focuserMediator = focuserMediator;
            this.weatherDataMediator = weatherDataMediator;
        }

        [JsonProperty]
        public AmbientTempSourceEnum AmbientTempSource {
            get => ambientTempSource;
            set {
                ambientTempSource = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public double MaxAmbientDeltaT {
            get => maxAmbientDeltaT;
            set {
                maxAmbientDeltaT = value;
                RaisePropertyChanged();
            }
        }

        private WaitForCooledMirror(WaitForCooledMirror copyMe) : this(copyMe.focuserMediator, copyMe.weatherDataMediator) {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            bool isCooling = true;
            string url = $"{pwi3UrlBase}&clientId={Pwi3ClientId}";

            while (isCooling) {
                try {
                    _ = await Utilities.HttpGetRequestAsync(Pwi3IpAddress, Pwi3Port, url, token);
                } catch {
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }

            return;
        }

        public override object Clone() {
            return new WaitForCooledMirror(this) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description,
                MaxAmbientDeltaT = MaxAmbientDeltaT,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(WaitForCooledMirror)}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            if (AmbientTempSource == AmbientTempSourceEnum.Focuser) {
                var info = focuserMediator.GetInfo();

                if (!info.Connected) {
                    i.Add("Focuser is not connnected");
                } else
                // Some focusers report a -127C temperature but actually lack a sensor. Sigh.
                if (double.IsNaN(info.Temperature) || info.Temperature < 100d) {
                    i.Add("Temperature is not available");
                }
            }

            if (AmbientTempSource == AmbientTempSourceEnum.WeatherSource) {
                var info = weatherDataMediator.GetInfo();

                if (!info.Connected) {
                    i.Add("Weather source is not connnected");
                } else if (double.IsNaN(info.Temperature)) {
                    i.Add("Temperature is not available");
                }
            }

            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged("Issues");
            }

            return i.Count == 0;
        }

        private string Pwi3ClientId { get; set; }
        private string Pwi3IpAddress { get; set; }
        private ushort Pwi3Port { get; set; }

        void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Pwi3ClientId":
                    Pwi3ClientId = Properties.Settings.Default.Pwi3ClientId;
                    break;
                case "Pwi3IpAddress":
                    Pwi3IpAddress = Properties.Settings.Default.Pwi3IpAddress;
                    break;
                case "Pwi3Port":
                    Pwi3Port = Properties.Settings.Default.Pwi3Port;
                    break;
            }
        }
    }
}