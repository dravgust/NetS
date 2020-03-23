using System;
using System.Threading;

namespace NetS.Core.Utilities
{
    internal class ActionDisposable : IDisposable
    {
        private Action _onEnter, _onLeave;

        public ActionDisposable(Action onEnter, Action onLeave)
        {
            _onEnter = onEnter;
            _onLeave = onLeave;
            _onEnter();
        }

        public void Dispose()
        {
            _onLeave();
        }
    }

    internal class ReaderWriterLock
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public IDisposable LockRead()
        {
            return new ActionDisposable(() => _lock.EnterReadLock(), () => _lock.ExitReadLock());
        }

        public IDisposable LockWrite()
        {
            return new ActionDisposable(() => _lock.EnterWriteLock(), () => _lock.ExitWriteLock());
        }

        internal bool TryLockWrite(out IDisposable locked)
        {
            locked = null;
            if (_lock.TryEnterWriteLock(0))
            {
                locked = new ActionDisposable(() =>
                {
                }, () => _lock.ExitWriteLock());
                return true;
            }
            return false;
        }
    }
}
