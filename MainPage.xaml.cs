using org.allseen.LSF.LampState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AllJoynApp1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AllJoynBusAttachment busAttachment = null;

        string lampId = "9830c3a7bc62d771b20899ace42611db";
        LampStateConsumer consumer = null;


        public MainPage()
        {
            this.InitializeComponent();

            slider.Maximum = uint.MaxValue;
            slider1.Maximum = uint.MaxValue;
            slider2.Maximum = uint.MaxValue;

            busAttachment = new AllJoynBusAttachment();

            // create a watcher for lamp state
            LampStateWatcher watcher = new LampStateWatcher(busAttachment);

            // to unsubscribe do -=
            watcher.Added += Watcher_Added;

            watcher.Start();

            Debug.Write("Started watching for producers");

        }

        private async void Watcher_Added(LampStateWatcher sender, AllJoynServiceInfo args)
        {
            Debug.Write("Found a producer");

            AllJoynAboutDataView aboutData = await AllJoynAboutDataView.GetDataBySessionPortAsync(args.UniqueName, busAttachment, args.SessionPort);

            if (aboutData != null && !string.IsNullOrWhiteSpace(aboutData.DeviceId) && string.Equals(aboutData.DeviceId, lampId))
            {
                LampStateJoinSessionResult result = await LampStateConsumer.JoinSessionAsync(args, sender);
                if (result.Status == AllJoynStatus.Ok)
                {
                    consumer = result.Consumer;

                    // this tells you whether or not the bulb is currrently on
                    LampStateGetOnOffResult onoffResult = await consumer.GetOnOffAsync();
                    if (onoffResult.Status == AllJoynStatus.Ok)
                    {
                        toggleSwitch.IsOn = onoffResult.OnOff;
                    }

                    // this gets the current value of the hue and displays it on the screen as it currently is
                    LampStateGetHueResult hueresult = await consumer.GetHueAsync();
                    if (hueresult.Status == AllJoynStatus.Ok)
                    {
                        slider.Value = hueresult.Hue;
                    }

                    LampStateGetSaturationResult saturationresult = await consumer.GetSaturationAsync();
                    if (saturationresult.Status == AllJoynStatus.Ok)
                    {
                        slider1.Value = saturationresult.Saturation;
                    }

                    LampStateGetBrightnessResult brightnessresult = await consumer.GetBrightnessAsync();
                    if (brightnessresult.Status == AllJoynStatus.Ok)
                    {
                        slider2.Value = brightnessresult.Brightness;
                    }
                }
            }
        }

        private async void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (consumer != null)
            {
                await consumer.SetOnOffAsync((sender as ToggleSwitch).IsOn);
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            if (consumer != null)
            {
                await consumer.SetHueAsync((uint)slider.Value);
            }
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            if (consumer != null)
            {
                await consumer.SetSaturationAsync((uint)slider1.Value);
            }
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            if (consumer != null)
            {
                await consumer.SetBrightnessAsync((uint)slider2.Value);
            }
        }
    }
}
