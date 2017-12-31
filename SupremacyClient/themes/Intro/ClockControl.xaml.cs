using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Supremacy.Client
{
    /// <summary>Interaction logic for ClockControl.xaml</summary>
    public partial class ClockControl : INotifyPropertyChanged
    {
        private static readonly DispatcherTimer _timer;

        public static readonly DependencyProperty ClockMarginProperty = DependencyProperty.Register(
            "ClockMargin",
            typeof(Thickness),
            typeof(ClockControl),
            new FrameworkPropertyMetadata(
                new Thickness(15, 3, 15, 10),
                FrameworkPropertyMetadataOptions.None));

        private double _utcMinutesOffset = double.NaN;

        /// <summary>
        ///   Initializes the
        ///   <see cref="ClockControl" />
        ///   class.
        /// </summary>
        static ClockControl()
        {
            _timer = new DispatcherTimer
                     {
                         Interval = new TimeSpan(0, 0, 0, 0, 500),
                         IsEnabled = true
                     };
        }

        /// <summary>
        ///   Initializes a new instance of the
        ///   <see cref="ClockControl" />
        ///   class.
        /// </summary>
        public ClockControl()
        {
            InitializeComponent();
            _timer.Tick += this.OnTimerTick;
        }

        public Thickness ClockMargin
        {
            get { return (Thickness)GetValue(ClockMarginProperty); }
            set { SetValue(ClockMarginProperty, value); }
        }

        /// <summary>Gets the current date time.</summary>
        /// <value>The current date time.</value>
        public DateTime CurrentDateTime
        {
            get
            {
                return (double.IsNaN(this._utcMinutesOffset))
                           ? DateTime.Now
                           : DateTime.UtcNow.AddMinutes(this._utcMinutesOffset);
            }
        }

        /// <summary>Gets the current hour as a number between 0 (inclusive) and 12 (exclusive).</summary>
        /// <value>The current hour.</value>
        public double CurrentHour
        {
            get
            {
                var now = this.CurrentDateTime;
                return ((now.Hour) % 12.0) + ((now.Minute) % 60.0) / 60.0;
            }
        }

        /// <summary>Gets the current minute as a number between 0 (inclusive) and 12 (exclusive).</summary>
        /// <value>The current minute.</value>
        public double CurrentMinute
        {
            get
            {
                var now = this.CurrentDateTime;
                return ((now.Minute) % 60.0) / 60.0 * 12.0;
            }
        }

        /// <summary>Gets the current second as a number between 0 (inclusive) and 12 (exclusive).</summary>
        /// <value>The current second.</value>
        public double CurrentSecond
        {
            get
            {
                var now = this.CurrentDateTime;
                return ((now.Second) % 60.0) / 60.0 * 12.0;
            }
        }

        /// <summary>Gets or sets the UTC minutes offset.</summary>
        /// <value>The UTC minutes offset.</value>
        public double UtcMinutesOffset
        {
            get { return this._utcMinutesOffset; }
            set
            {
                this._utcMinutesOffset = value;
                OnPropertyChanged("CurrentHour");
                OnPropertyChanged("CurrentMinute");
                OnPropertyChanged("CurrentSecond");
            }
        }

        #region INotifyPropertyChanged Members
        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        /// <summary>
        ///   Raises the
        ///   <see cref="PropertyChanged" />
        ///   event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///   Handles the
        ///   <c>Tick</c>
        ///   event of the
        ///   <see cref="_timer" />
        ///   object.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///   The
        ///   <see cref="EventArgs" />
        ///   instance containing the event data.
        /// </param>
        private void OnTimerTick(object sender, EventArgs e)
        {
            OnPropertyChanged("CurrentHour");
            OnPropertyChanged("CurrentMinute");
            OnPropertyChanged("CurrentSecond");
            OnPropertyChanged("CurrentDateTime");
        }

        /// <summary>
        ///   Raises the
        ///   <see cref="PropertyChanged" />
        ///   event.
        /// </summary>
        /// <param name="e">
        ///   The
        ///   <see cref="PropertyChangedEventArgs" />
        ///   instance containing the event data.
        /// </param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var eventHandlers = this.PropertyChanged;
            if (null != eventHandlers)
                eventHandlers(this, e);
        }
    }
}