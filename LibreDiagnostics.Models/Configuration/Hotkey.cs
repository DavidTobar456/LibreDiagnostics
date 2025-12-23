/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibreDiagnostics.Models.Configuration
{
    public class HotKey : ViewModelBase
    {
        #region Constructor

        public HotKey(HotKeyID id)
        {
            ID = id;
        }

        #endregion

        #region Properties

        [JsonIgnore]
        public bool IsEmpty => Key == 0 && Modifiers == 0;

        [JsonIgnore]
        public bool IsValid => 
            (Key != 0 && Modifiers != 0)
         || (Key != 0);

        HotKeyID _ID;
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public HotKeyID ID
        {
            get { return _ID; }
            set { SetField(ref _ID, value); }
        }

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
