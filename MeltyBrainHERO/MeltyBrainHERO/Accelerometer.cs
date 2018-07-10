namespace MeltyBrainHERO
{
    using Microsoft.SPOT.Hardware;

    public class Accelerometer
    {
        public AnalogInput Source { get; set; }
        private CircularBuffer readings = new CircularBuffer(5);
        private double baseAcceleration = 0;
    

        public Accelerometer(AnalogInput source)
        {
            this.Source = source;
        }

        public void Tare()
        {
            this.baseAcceleration = this.GetAccelerationRaw();
        }

        public double ReadRaw()
        {
            return this.Source.Read();
        }

        public double Read()
        {
            readings.PushFront(this.ReadRaw());
            return readings.Average;
        }

        public double GetAccelerationRaw()
        {
            double voltage = this.ReadRaw();

            return convertVoltageToAcceleration(voltage);
        }

        public double GetAcceleration()
        {
            double voltage = this.Read();
            return convertVoltageToAcceleration(voltage) - baseAcceleration;
        }

        private double convertVoltageToAcceleration(double voltage)
        {
            // TODO: Some math

            return 0;
        }
    }
}
