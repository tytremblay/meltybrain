using System;
using Microsoft.SPOT;

namespace MeltyBrainHERO
{
    public class CircularBuffer
    {
        private double[] values;

        public CircularBuffer(int size)
        {
            this.values = new double[size];
        }

        public void PushEnd(double value)
        {
            for(int i = 0; i < values.Length; i++)
            {
                values[i] = values[i + 1];
            }
            values[values.Length - 1] = value;
        }

        public void PushFront(double value)
        {
            for(int i = values.Length; i > 0; i--)
            {
                values[i] = values[i - 1];
            }
            values[0] = value;
        }

        public double Average
        {
            get
            {
                double sum = 0;
                foreach(double v in values)
                {
                    sum += v;
                }

                return sum / values.Length;
            }
        }
    }
}
