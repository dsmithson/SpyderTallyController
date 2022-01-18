using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using System.Net;

namespace SpyderTallyControllerWebApp.Models
{
    public class DisplayRepository : IDisplayRepository
    {
        private const int filledCircleLocation = 0;
        private const int emptyCircleLocation = 1;

        private readonly I2cDevice i2c;
        private readonly Pcf8574 driver;
        private readonly Lcd1602 lcd;

        private readonly IRelayRepository relayRepository;

        private DisplayMode displayMode = DisplayMode.Normal;
        private string manualTextLine1 = "";
        private string manualTextLine2 = "";

        public DisplayRepository(IRelayRepository relayRepository)
        {
            this.relayRepository = relayRepository;
            this.relayRepository.RelayStatusChanged += RelayRepository_RelayStatusChanged;

            //Initialize our display
            i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
            driver = new Pcf8574(i2c);
            lcd = new Lcd1602(registerSelectPin: 0,
                enablePin: 2,
                dataPins: new int[] { 4, 5, 6, 7 },
                backlightPin: 3,
                backlightBrightness: 0.1f,
                readWritePin: 1,
                controller: new GpioController(PinNumberingScheme.Logical, driver));

            //Create filled 0 charcater
            byte[] filledCircle = new byte[]
            {
                0x0E,   // 0XXX0
                0x1F,   // XXXXX
                0x1F,   // XXXXX
                0x1F,   // XXXXX
                0x1F,   // XXXXX
                0x1F,   // XXXXX
                0x1F,   // XXXXX
                0x0E,   // 0XXX0
            };
            lcd.CreateCustomCharacter(filledCircleLocation, filledCircle);

            byte[] emptyCircle = new byte[]
            {
                0x0E,   // 0XXX0
                0x11,   // X000X
                0x11,   // X000X
                0x11,   // X000X
                0x11,   // X000X
                0x11,   // X000X
                0x11,   // X000X
                0x0E,   // 0XXX0
            };
            lcd.CreateCustomCharacter(emptyCircleLocation, emptyCircle);


            lcd.Clear();
            lcd.DisplayOn = true;
            UpdateDisplay();
        }

        private void RelayRepository_RelayStatusChanged(object sender, EventArgs e)
        {
            if (displayMode == DisplayMode.Normal)
                UpdateDisplay();
        }

        public void SetDisplayMode(DisplayMode mode)
        {
            if (this.displayMode != mode)
            {
                this.displayMode = mode;
                UpdateDisplay();
            }
        }

        public void SetText(string line1, string line2)
        {
            this.displayMode = DisplayMode.TwoLineManual;
            manualTextLine1 = line1;
            manualTextLine2 = line2;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            string text1, text2;
            if (displayMode == DisplayMode.Normal)
            {
                text1 = Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && ip.GetAddressBytes()[0] != 127)
                    .FirstOrDefault()?
                    .ToString() ?? "<No IP>";

                text2 = String.Join(null, relayRepository.GetRelayStatus().Select(isOn => isOn ? (char)filledCircleLocation : (char)emptyCircleLocation));

                //Debug
                Console.WriteLine(string.Join(Environment.NewLine, Dns.GetHostAddresses(Dns.GetHostName()).ToList()));
            }
            else
            {
                text1 = manualTextLine1;
                text2 = manualTextLine2;
            }
            lcd.SetCursorPosition(0, 0);
            lcd.Write(text1);

            lcd.SetCursorPosition(0, 1);
            lcd.Write(text2);
        }
    }
}
