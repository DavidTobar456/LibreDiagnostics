/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;
using Newtonsoft.Json;

namespace LibreDiagnostics.Models.Configuration
{
    public class Hotkey : ViewModelBase
    {
        #region Properties

        [JsonIgnore]
        public bool IsEmpty => Key == 0 && Modifiers == 0;

        [JsonIgnore]
        public bool IsValid => Key != 0 && Modifiers != 0;

        int _Key;
        [JsonProperty]
        public int Key
        {
            get { return _Key; }
            set
            {
                SetField(ref _Key, value);
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        int _Modifiers;
        [JsonProperty]
        public int Modifiers
        {
            get { return _Modifiers; }
            set
            {
                SetField(ref _Modifiers, value);
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        #endregion
    }
}
