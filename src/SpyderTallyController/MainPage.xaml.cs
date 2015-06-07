using PCLStorage;
using Spyder.Client;
using Spyder.Client.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
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

namespace SpyderTallyController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string spyderIP = "192.168.1.100";
        string[] sourcesToTallyOn = new string[] { "PC 1", "PC 2", "DDRA", "Camera 2", "PC GFX I", "IMAG 1", "Source 7", "Source 8" };

        GpioPin[] pins;
        SpyderClientManager spyderManager;
        BindableSpyderClient spyderServer;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var gpio = GpioController.GetDefault();
            if (gpio != null)
            {
                //Initialize our array of pins
                int[] pinNumbers = new int[] { 18, 27, 22, 23, 24, 25, 5, 6 };
                pins = new GpioPin[pinNumbers.Length];
                for (int i = 0; i < pinNumbers.Length; i++)
                {
                    var pin = gpio.OpenPin(pinNumbers[i]);
                    pin.Write(GpioPinValue.High);
                    pin.SetDriveMode(GpioPinDriveMode.Output);
                    pins[i] = pin;
                }
            }

            //Initialize our spyder client
            IFolder localCache = await FileSystem.Current.LocalStorage.CreateFolderAsync("Server", CreationCollisionOption.OpenIfExists);
            spyderManager = new SpyderClientManager(localCache);
            spyderManager.ServerListChanged += SpyderManager_ServerListChanged;
            await spyderManager.Startup();
        }

        private void SpyderManager_ServerListChanged(object sender, EventArgs e)
        {
            if (spyderServer == null)
            {
                spyderServer = spyderManager.GetServer(spyderIP);
                if (spyderServer != null)
                {
                    spyderServer.DrawingDataReceived += SpyderServer_DrawingDataReceived;
                }
            }
        }

        private void SpyderServer_DrawingDataReceived(object sender, Spyder.Client.Net.Notifications.DrawingDataReceivedEventArgs e)
        {
            var drawingData = e.DrawingData;

            //Build a map of sources
            var sourcesInProgram = (from layer in drawingData.DrawingKeyFrames.Values
                                    join pixelSpace in drawingData.PixelSpaces.Values on layer.PixelSpaceID equals pixelSpace.ID
                                    where layer.IsVisible && pixelSpace.Scale == 1f
                                    select layer.Source)
                                    .Distinct()
                                    .ToList();

            //Let's update our tallies
            for (int i = 0; i < pins.Length; i++)
            {
                if (i >= sourcesToTallyOn.Length)
                {
                    pins[i].Write(GpioPinValue.High);
                }
                else
                {
                    string source = sourcesToTallyOn[i];
                    if (!string.IsNullOrEmpty(source) && sourcesInProgram.Contains(source))
                    {
                        pins[i].Write(GpioPinValue.Low);
                    }
                    else
                    {
                        pins[i].Write(GpioPinValue.High);
                    }
                }
            }
        }
    }
}
