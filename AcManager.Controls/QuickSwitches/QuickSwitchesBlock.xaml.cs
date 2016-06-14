﻿using System;
using System.Windows;
using AcManager.Tools.Helpers;
using FirstFloor.ModernUI.Helpers;

namespace AcManager.Controls.QuickSwitches {
    public partial class QuickSwitchesBlock {
        public QuickSwitchesBlock() {
            InitializeComponent();
            Rebuild();

            SettingsHolder.Drive.PropertyChanged += Drive_PropertyChanged;
        }

        private void Drive_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(SettingsHolder.Drive.QuickSwitches) ||
                    e.PropertyName == nameof(SettingsHolder.Drive.QuickSwitchesList)) {
                Rebuild();
            }
        }

        private void Rebuild() {
            List.Items.Clear();
            if (!SettingsHolder.Drive.QuickSwitches) return;

            var active = SettingsHolder.Drive.QuickSwitchesList;
            foreach (var key in active) {
                FrameworkElement item;
                try {
                    item = (FrameworkElement)FindResource(key);
                } catch (Exception e) {
                    Logging.Write("Can’t find widget: " + key + ", " + e);
                    continue;
                }

                List.Items.Add(item);
            }
        }
    }
}
