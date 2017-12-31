using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Supremacy.Utility
{
    // We use plenty of interlocked operations on volatile fields below.  Safe.
#pragma warning disable 0420

    /// <summary>
    /// A very lightweight reader/writer lock.  It uses a single word of memory, and
    /// only spins when contention arises (no events are necessary).
    /// </summary>
    public sealed class ReaderWriterSpinLock
    {
        private volatile int _writer;
        private volatile ReadEntry[] _readers = new ReadEntry[Environment.ProcessorCount * 16];

        private int ReadLockIndex
        {
            get { return Thread.CurrentThread.ManagedThreadId % _readers.Length; }
        }

        public void EnterReadLock()
        {
            var spinLock = new SpinLock();
            var threadId = ReadLockIndex;

            // Wait until there are no writers.
            while (true)
            {
                while (_writer == 1)
                    spinLock.SpinOnce();

                // Try to take the read lock.
                Interlocked.Increment(ref _readers[threadId].Taken);

                // Success, no writer, proceed.
                if (_writer == 0)
                    break;

                // Back off, to let the writer go through.
                Interlocked.Decrement(ref _readers[threadId].Taken);
            }
        }

        public void EnterWriteLock()
        {
            var spinLock = new SpinLock();
            var threadId = ReadLockIndex;

            while (true)
            {
                if ((_writer == 0) && (Interlocked.Exchange(ref _writer, 1) == 0))
                {
                    // We now hold the write lock, and prevent new readers.
                    // But we must ensure no readers exist before proceeding.
                    for (var i = 0; i < _readers.Length; i++)
                    {
                        if (i == threadId)
                            continue;
                        while (_readers[i].Taken != 0) 
                            spinLock.SpinOnce();
                    }
                    break;
                }

                // We failed to take the write lock; wait a bit and retry.
                spinLock.SpinOnce();
            }
        }

        public void ExitReadLock()
        {
            // Just note that the current reader has left the lock.
            Interlocked.Decrement(ref _readers[ReadLockIndex].Taken);
        }

        public void ExitWriteLock()
        {
            // No need for a CAS.
            _writer = 0;
        }

        [StructLayout(LayoutKind.Sequential, Size = 128)]
        private struct ReadEntry
        {
            internal volatile int Taken;
        }

        private struct SpinLock
        {
            private int _count;

            internal void SpinOnce()
            {
                if (_count++ > 32)
                    Thread.Sleep(0);
                else if (_count > 12)
                    AsyncHelper.Yield();
                else
                    Thread.SpinWait(2 << _count);
            }
        }
    }
}