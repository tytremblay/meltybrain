namespace MeltyBrainHERO
{
    using CTRE.Phoenix.MotorControl;
    using CTRE.Phoenix.MotorControl.CAN;
    using System;

    public class MeltyDrive
    {
        public VictorSPX LeftMotor { get; set; }
        public VictorSPX RightMotor { get; set; }
        public Accelerometer Accelerometer { get; set; }
        public LED Led { get; set; } = new LED();

        // Configurations
        public double AccelDistFromCenter { get; set; } = 0;  // cm
        public double TrackingScalar { get; set; } = 1.0;
        public double ForwardScalar { get; set; } = 1.0;
        public double BackwardScalar { get; set; } = 1.0;
        public double TurnScalar { get; set; } = 0.01;
        public double LedAdjustPercentage { get; set; } = 0.63;
        public double MinRpm { get; set; } = 500;
        public double MaxRpm { get; set; } = 5000;
        public double MaxAccel { get; set; } = 250; // g

        // States

        public double Throttle { get; set; } = 0.0;
        public double MoveVal { get; set; } = 0.0;
        public double TurnVal { get; set; } = 0.0;

        public double MaxPower { get; set; } = 0.25;
        public double CurrentRpm { get; set; } = 0;
        public double CurrentAccel { get; set; } = 0;
        public double MaxObservedRpm { get; set; } = 0;
        public double MaxObservedAcceleration { get; set; } = 0;
        public double FullSpinTimeMs { get; set; } = 0;
        public double HalfSpinTimeMs { get; set; } = 0;
        public bool FullPowerSpin { get; set; } = false;

        public double BrakingLength { get; set; } = 0.0;
        public double BeginBrake { get; set; } = 0.0;
        public double EndBrake { get; set; } = 0.0;
        public double PowerKillLength { get; set; } = 0.0;
        public double PowerKillPart1 { get; set; } = 0.0;
        public double PowerKillPart2 { get; set; } = 0.0;

        public bool FlipPower { get; set; } = false;

        public CTRE.Phoenix.Stopwatch stopwatch { get; private set; } = new CTRE.Phoenix.Stopwatch();

        public MeltyDrive(VictorSPX left, VictorSPX right, Accelerometer accel, double accelDistFromCenter)
        {
            this.LeftMotor = left;
            this.RightMotor = right;
            this.Accelerometer = accel;
            this.AccelDistFromCenter = accelDistFromCenter;
        }

        public void CalculateRpm()
        {
            CurrentAccel = Accelerometer.GetAcceleration();
            if (CurrentAccel > MaxObservedAcceleration)
            {
                MaxObservedAcceleration = CurrentAccel;
            }
            // calculate RPM from g's - derived from "G = 0.00001118 * r * RPM^2"
            double rpm = Math.Pow(((CurrentAccel * 89445) / this.AccelDistFromCenter), 0.5);
            if (rpm > MaxObservedRpm)
            {
                MaxObservedRpm = rpm;
            }

            this.CurrentRpm = rpm;
        }

        public void CalculateSpinTimes(double moveVal, double turnVal)
        {
            double fullSpinTime = CurrentRpm / 60;
            if (fullSpinTime == 0)
            {
                // prevent divide by 0
                fullSpinTime = 1;
            }

            fullSpinTime = 1000 / fullSpinTime;

            if (moveVal >= 0)
            {
                this.FullSpinTimeMs = ForwardScalar * fullSpinTime;
            }
            else
            {
                this.FullSpinTimeMs =  BackwardScalar * fullSpinTime;
            }

            if (turnVal != 0)
            {
                this.FullSpinTimeMs *= 1 - turnVal * TurnScalar;
            }

            double halfSpinMs = this.FullSpinTimeMs / 2;
            if (halfSpinMs > 200)
            {
                halfSpinMs = 200;
            }
            if (halfSpinMs < 5)
            {
                halfSpinMs = 5;
            }

            this.HalfSpinTimeMs = halfSpinMs;
        }

        public void CalculateLedTimesMs()
        {
            this.Led.OnTimeMs = this.FullSpinTimeMs * this.LedAdjustPercentage;
            this.Led.OffTimeMs = FullSpinTimeMs / 3 + this.Led.OnTimeMs;
        }

        public void Calculate(double throttle, double moveVal, double turnVal)
        {
            stopwatch = new CTRE.Phoenix.Stopwatch();
            stopwatch.Start();

            this.Led.FlashyLed = false;

            CalculateRpm();

            CalculateSpinTimes(moveVal, turnVal);

            FullPowerSpin = false;

            if (CurrentRpm < MinRpm)
            {
                FullPowerSpin = true;
            }

            if (CurrentAccel > MaxAccel|| CurrentRpm > MaxRpm)
            {
                throttle = .1;
            }

            if (throttle > .5)
            {
                this.Led.FlashyLed = true;

                BrakingLength = HalfSpinTimeMs * 25;
                BeginBrake = HalfSpinTimeMs / 2 - BrakingLength;
                EndBrake = HalfSpinTimeMs / 2 + BrakingLength;

                if (BeginBrake < 1)
                {
                    BeginBrake = 1;
                }

                PowerKillPart1 = 0;
                PowerKillPart2 = HalfSpinTimeMs;
            }

            if (throttle <= .5)
            {
                BeginBrake = 1;
                EndBrake = HalfSpinTimeMs;

                PowerKillLength = (50 - throttle * 100) * HalfSpinTimeMs / 150;
                PowerKillPart1 = PowerKillLength;
                PowerKillPart2 = HalfSpinTimeMs - PowerKillLength;
           
            }

            if (FullPowerSpin)
            {
                EndBrake = 1;
                BeginBrake = 0;
                PowerKillPart1 = 0;
                PowerKillPart2 = HalfSpinTimeMs;
            }

            this.Throttle = throttle;
            this.MoveVal = moveVal;
            this.TurnVal = TurnVal;            
        }

        public void DoSpin180(bool phaseOne)
        {
            while(stopwatch.DurationMs < HalfSpinTimeMs)
            {
                if (stopwatch.DurationMs < BeginBrake)
                {
                    SetMotors(MaxPower, MaxPower);
                }
                if (stopwatch.DurationMs > EndBrake)
                {
                    SetMotors(MaxPower, MaxPower);
                }

                this.Led.Ref = stopwatch.DurationMs + this.Led.HoldOver;

                if (stopwatch.DurationMs > BeginBrake && stopwatch.DurationMs < EndBrake)
                {
                    if (this.MoveVal == 0)
                    {
                        if (this.FlipPower)
                        {
                            if (phaseOne)
                            {
                                this.LeftMotorOn();
                            }
                            else
                            {
                                this.RightMotorOn();
                            }
                        }
                        else
                        {
                            if (phaseOne)
                            {
                                this.RightMotorOn();
                            }
                            else
                            {
                                this.LeftMotorOn();
                            }
                        }
                    }

                    if (this.MoveVal > 0)
                    {
                        if (phaseOne)
                        {
                            LeftMotorOn();
                        }
                        else
                        {
                            RightMotorOn();
                        }

                    }
                }

                if(stopwatch.DurationMs > EndBrake)
                {
                    this.MotorsOn();
                }

                if(stopwatch.DurationMs < PowerKillPart1)
                {
                    this.MotorsOff();
                }

                if(stopwatch.DurationMs > PowerKillPart2)
                {
                    this.MotorsOff();
                }

                this.Led.Update(stopwatch.DurationMs);
            }
        }

        public void LeftMotorOn()
        {
            this.SetMotors(this.MaxPower, 0);
        }

        public void RightMotorOn()
        {
            this.SetMotors(0, this.MaxPower);
        }

        public void MotorsOn()
        {
            this.SetMotors(this.MaxPower, this.MaxPower);
        }

        public void MotorsOff()
        {
            this.SetMotors(0, 0);
        }

        public void SetMotors(double leftPower, double rightPower)
        {
            this.LeftMotor.Set(ControlMode.PercentOutput, leftPower);
            this.RightMotor.Set(ControlMode.PercentOutput, -rightPower);
        }
    }
}
