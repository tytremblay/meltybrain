namespace MeltyBrainHERO
{
    using CTRE.Phoenix;
    using CTRE.Phoenix.Controller;

    public class UI
    {
        private GameController controller;

        public bool IsConnected { get; private set; } = false;
        public double Deadband { get; set; } = 0.05;

        public void Acquire()
        {
            if (this.controller == null)
            {
                this.controller = new GameController(UsbHostDevice.GetInstance());
                this.IsConnected = this.controller != null;
            }
        }

        public double RightX { get
            {
                var raw = this.controller.GetAxis(0);

                return HandleDeadband(raw);
            }
        }

        public double RightY
        {
            get
            {
                var raw = this.controller.GetAxis(1);

                return HandleDeadband(raw);
            }
        }

        public double LeftX
        {
            get
            {
                var raw = this.controller.GetAxis(2);

                return HandleDeadband(raw);
            }
        }

        public double LeftY
        {
            get
            {
                var raw = this.controller.GetAxis(3);

                return HandleDeadband(raw);
            }
        }


        public double HandleDeadband(double value)
        {
            if (value < -this.Deadband && value > this.Deadband)
            {
                // outside of deadband
                return value;
            }
            else
            {
                return 0;
            }
        }
    }
}
