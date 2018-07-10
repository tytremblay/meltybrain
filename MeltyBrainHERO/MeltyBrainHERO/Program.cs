namespace MeltyBrainHERO
{
    using System.Threading;

    using CTRE.Phoenix;
    using CTRE.Phoenix.MotorControl.CAN;
    using Microsoft.SPOT.Hardware;
    public class Program
    {
        /* create a talon */
        static VictorSPX right = new VictorSPX(2);
        static VictorSPX left = new VictorSPX(1);
        static AnalogInput accelPin = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin3);

        static double accelDistFromCetner = 2.54; // cm

        static Accelerometer accelerometer = new Accelerometer(accelPin);
        static UI ui = new UI();
        static MeltyDrive drive = new MeltyDrive(left, right, accelerometer, accelDistFromCetner);


        public static void Main()
        {
            /* loop forever */
            while (true)
            {
                if (!ui.IsConnected)
                {
                    ui.Acquire();
                }

                double throttle = ui.LeftY;
                double moveVal = ui.RightY;
                double turnVal = ui.RightX;

                drive.FlipPower = !drive.FlipPower;

                drive.Led.HoldOver = 0;

                drive.Calculate(throttle, moveVal, turnVal);

                drive.DoSpin180(true);

                drive.Led.HoldOver = drive.stopwatch.DurationMs;

                drive.Calculate(throttle, moveVal, turnVal);

                drive.DoSpin180(false);

                /* feed watchdog to keep Talon's enabled */
                CTRE.Phoenix.Watchdog.Feed();
            }
        }
    }
}