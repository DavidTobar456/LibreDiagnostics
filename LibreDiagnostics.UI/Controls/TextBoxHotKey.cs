/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using BlackSharp.Core.Extensions;
using LibreDiagnostics.Models.Configuration;
using System.ComponentModel;

namespace LibreDiagnostics.UI.Controls
{
    public class TextBoxHotKey : TextBox
    {
        #region Constructor

        static TextBoxHotKey()
        {
            FocusableProperty.OverrideDefaultValue<TextBoxHotKey>(true);
            IsReadOnlyProperty.OverrideDefaultValue<TextBoxHotKey>(true);
        }

        #endregion

        #region XAML Properties

        public static readonly new StyledProperty<HotKey> TextProperty =
            AvaloniaProperty.Register<TextBoxHotKey, HotKey>(
                nameof(Text),
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        #endregion

        #region Properties

        public new HotKey Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion

        #region Protected

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TextProperty)
            {
                UpdateDisplayedText();

                if (change.OldValue is HotKey oldHotkey)
                {
                    oldHotkey.PropertyChanged -= HotkeyPropertyChanged;
                }

                if (change.NewValue is HotKey newHotkey)
                {
                    newHotkey.PropertyChanged += HotkeyPropertyChanged;
                }
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Text == null)
            {
                return;
            }

            //Backspace = delete
            if (e.Key == Key.Back)
            {
                Text.Key = (int)Key.None;
                Text.Modifiers = (int)KeyModifiers.None;
                e.Handled = true;
                return;
            }

            if (e.KeyModifiers != KeyModifiers.None &&
                e.Key.AnyOf(Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift))
            {
                Text.Key = (int)Key.None;
                Text.Modifiers = (int)e.KeyModifiers;
            }
            else
            {
                Text.Key = (int)e.Key;
                Text.Modifiers = (int)e.KeyModifiers;
            }

            e.Handled = true;
        }

        #endregion

        #region Private

        void HotkeyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateDisplayedText();
        }

        void UpdateDisplayedText()
        {
            if (Text == null || Text.IsEmpty)
            {
                base.Text = string.Empty;
            }
            else
            {
                var keys = new List<string>();

                var modifiers = (KeyModifiers)Text.Modifiers;
                var key = (Key)Text.Key;

                if (modifiers.HasFlag(KeyModifiers.Control))
                {
                    keys.Add(KeyModifiers.Control.ToString());
                }

                if (modifiers.HasFlag(KeyModifiers.Shift))
                {
                    keys.Add(KeyModifiers.Shift.ToString());
                }

                if (modifiers.HasFlag(KeyModifiers.Alt))
                {
                    keys.Add(KeyModifiers.Alt.ToString());
                }

                if (modifiers.HasFlag(KeyModifiers.Meta))
                {
                    keys.Add(KeyModifiers.Meta.ToString());
                }

                if (key != Key.None)
                {
                    keys.Add(key.ToString());
                }

                base.Text = string.Join(" + ", keys);
            }
        }

        #endregion
    }
}
