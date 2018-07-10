using System;
using Microsoft.SPOT;

namespace MeltyBrainHERO
{
    public class LED
    {
        public bool IsOn { get; set; } = false;
        public bool FlashyLed { get; set; } = false;
        public double Ref { get; set; } = 0.0;
        public long HoldOver { get; set; } = 0;
        public double OffTimeMs { get; set; } = 0;
        public double OnTimeMs { get; set; } = 0;
        public bool LedOn { get; set; } = false;

        public void Update(double stopwatchDurationMs)
        {
            if (OnTimeMs > OffTimeMs)
            {
                LedOn = true;
                if (Ref > OffTimeMs) LedOn = false;
                if (Ref > OnTimeMs) LedOn = true;
            }


            if (OffTimeMs > OnTimeMs)
            {
                LedOn = false;
                if (Ref > OnTimeMs) LedOn = true;
                if (Ref > OffTimeMs) LedOn = false;
            }


            if (LedOn)
            {
                //flash the LED if we're in flashy mode - otherwise it's just on
                if (FlashyLed)
                {

                    if ((stopwatchDurationMs / 160) % 2 == 0) this.TurnOn(); else this.TurnOff();
                }
                else
                {
                    this.TurnOn();
                }
            }
            else
            {
                this.TurnOff();
            }
        }

        public void TurnOn()
        {
            // TODO: Turn on the LED

            this.IsOn = true;
        }

        public void TurnOff()
        {
            // TODO: turn off the LED

            this.IsOn = false;
        }
    }
}
