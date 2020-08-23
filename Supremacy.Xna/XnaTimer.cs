using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace Supremacy.Xna
{
    public class XnaTimer : DispatcherObject
    {
        private readonly DispatcherTimer _timer;
        private bool _usingTimer;
        private bool _started;

        public event EventHandler Tick;

        public XnaTimer()
        {
            _timer = XnaHelper.CreateRenderTimer();
            _timer.Tick += OnTick;
        }

        public void Start(bool usingHardwareDevice)
        {
            CheckAccess();

            if (_started)
            {
                return;
            }

            _started = true;

            if (usingHardwareDevice)
            {
                CompositionTarget.Rendering += OnTick;
                return;
            }

            _usingTimer = true;
            _timer.Start();
        }

        public void Stop()
        {
            CheckAccess();

            if (!_started)
            {
                return;
            }

            if (_usingTimer)
            {
                _timer.Stop();
            }
            else
            {
                CompositionTarget.Rendering -= OnTick;
            }

            _usingTimer = false;
            _started = false;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Tick?.Invoke(this, EventArgs.Empty);
        }
    }
}