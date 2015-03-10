using System;

namespace STM.Interfaces
{
    public interface ILock
    {
        void Lock();
        void UnLock();
        bool TryLock(int milisecs);
        bool TryLock(TimeSpan span);
        bool IsLocked { get; }
        bool IsLockedByCurrentThread { get; }
    }
}