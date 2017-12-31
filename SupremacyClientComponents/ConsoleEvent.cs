namespace Supremacy.Client
{
    public class ConsoleEvent
    {
        private readonly object _output;

        public ConsoleEvent(object output)
        {
            _output = output;
        }

        public object Output
        {
            get { return _output; }
        }
    }
}