﻿using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Decoder;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class EqualizerPage : AbstractPage
	{
		public override Pages PageType => Pages.Equalizer;
		
		private readonly SwitchListItem _eqSwitch;
		private readonly SliderListItem _presetSlider;
		
		public EqualizerPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_eqSwitch = this.FindControl<SwitchListItem>("EqToggle");
			_presetSlider = this.FindControl<SliderListItem>("EqPreset");
			
			SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
			SPPMessageHandler.Instance.OtherOption += InstanceOnOtherOption;
			
			Loc.LanguageUpdated += UpdateStrings;
			UpdateStrings();
		}

		private async void InstanceOnOtherOption(object? sender, TouchOptions e)
		{
			ICustomAction action = e == TouchOptions.OtherL ? 
				SettingsProvider.Instance.CustomActionLeft : SettingsProvider.Instance.CustomActionRight;
			
			switch (action.Action)
			{
				case CustomAction.Actions.EnableEqualizer:
					_eqSwitch.IsChecked = !_eqSwitch.IsChecked;
					await MessageComposer.SetEqualizer(_eqSwitch.IsChecked, (EqPreset)_presetSlider.Value, false);
					break;
				
				case CustomAction.Actions.SwitchEqualizerPreset:
					_eqSwitch.IsChecked = true;
					var newVal = _presetSlider.Value + 1;
					if (newVal >= 5)
						newVal = 0;

					_presetSlider.Value = newVal;
					await MessageComposer.SetEqualizer(_eqSwitch.IsChecked, (EqPreset)_presetSlider.Value, false);
					break;
			}
		}

		private void InstanceOnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
		{
			if (BluetoothImpl.Instance.ActiveModel == Models.Buds)
			{
				_eqSwitch.IsChecked = e.EqualizerEnabled;
				
				var preset = e.EqualizerMode;
				if (preset >= 5)
				{
					preset -= 5;
				}

				_presetSlider.Value = preset;
			}
			else
			{
				_eqSwitch.IsChecked = e.EqualizerMode != 0;
				if (e.EqualizerMode == 0)
				{
					_presetSlider.Value = 2;
				}
				else
				{
					_presetSlider.Value = e.EqualizerMode - 1;
				}
			}
		}

		private void UpdateStrings()
		{
			_presetSlider.SubtitleDictionary = new Dictionary<int, string>()
			{
				{ 0, Loc.Resolve("eq_bass") },
				{ 1, Loc.Resolve("eq_soft") },
				{ 2, Loc.Resolve("eq_dynamic") },
				{ 3, Loc.Resolve("eq_clear") },
				{ 4, Loc.Resolve("eq_treble") }
			};
		}

		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}

		private async void EqToggle_OnToggled(object? sender, bool e)
		{
			await MessageComposer.SetEqualizer(_eqSwitch.IsChecked, (EqPreset)_presetSlider.Value, false);
		}

		private async void EqPreset_OnChanged(object? sender, int e)
		{
			await MessageComposer.SetEqualizer(_eqSwitch.IsChecked, (EqPreset)_presetSlider.Value, false);
		}
	}
}
