using System;
using Supremacy.Annotations;

namespace Supremacy.Client.Audio
{
    public interface IAudioGrouping : IDisposable
    {
        float Volume { get; set; }
        IAudioGrouping Parent { get; set; }
    }

    public class FMODGrouping : IAudioGrouping
    {
        #region Fields
        private bool _isDisposed = false;
        private bool _external = false;
        protected FMODAudioEngine _engine = null;
        protected FMOD.ChannelGroup _channelGroup = null;
        protected IAudioGrouping _parent = null;
        #endregion

        #region Properties
        public float Volume
        {
            get
            {
                float volume = 0;
                lock (_engine.Lock)
                {
                    FMODErr.Check(_channelGroup.getVolume(ref volume));
                }
                return volume;
            }
            set
            {
                if (value < 0) value = 0.0f;
                else if (value > 1) value = 1.0f;

                lock (_engine.Lock)
                {
                    _channelGroup.setVolume(value);
                }
            }
        }

        public IAudioGrouping Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                lock (_engine.Lock)
                {
                    _parent = value;
                    if (_channelGroup != null)
                    {
                        var parent = (_parent as FMODGrouping);
                        if (parent != null && parent._channelGroup != null)
                            parent._channelGroup.addGroup(_channelGroup);
                        else
                        {
                            // return to master http://www.fmod.org/questions/question/forum-27115
                            (_engine.Master as FMODGrouping)._channelGroup.addGroup(_channelGroup);
                        }
                    }
                }
            }
        }

        internal FMOD.ChannelGroup ChannelGroup { get { return _channelGroup; } }
        #endregion

        #region Construction & Lifetime
        internal FMODGrouping([NotNull] FMODAudioEngine engine, string name)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            _engine = engine;
            FMODErr.Check(engine.System.createChannelGroup(name, ref _channelGroup));
        }

        internal FMODGrouping([NotNull] FMODAudioEngine engine, [NotNull] FMOD.ChannelGroup channelGroup)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (channelGroup == null)
                throw new ArgumentNullException("channelGroup");

            _engine = engine;
            _channelGroup = channelGroup;
            _external = true;
        }

        public void Dispose()
        {
            if (_external)
                return;

            lock (_engine.Lock)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                _channelGroup.release();
                _channelGroup = null;
                _engine.RemoveGrouping(this);
            }
        }
        #endregion
    }
}
