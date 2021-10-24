#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using System.ComponentModel;

namespace DaleGhent.NINA.PlaneWaveTools.Utility {

    [TypeConverter(typeof(EnumDescTypeConverter))]
    public enum AmbientTempSourceEnum {
        [Description("Delta T")]
        DeltaT,

        [Description("EFA")]
        EFA,

        [Description("Focuser")]
        Focuser,

        [Description("Weather Source")]
        WeatherSource,
    }
}