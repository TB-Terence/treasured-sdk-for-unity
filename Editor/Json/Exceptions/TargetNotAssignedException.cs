using System;

namespace Treasured.UnitySdk
{
    public class TargetNotAssignedException : Exception
    {
        public TreasuredObject Object { get; set; }

        public TargetNotAssignedException(string message, TreasuredObject obj) : base(message)
        {
            Object = obj;
        }
    }
}
