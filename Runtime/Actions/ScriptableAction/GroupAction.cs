namespace Treasured.UnitySdk
{
    [API("group")]
    public class GroupAction : ScriptableAction
    {
        public enum ExecutionMode
        {
            Sequence,
            Parallel,
            Race
        }

        public ExecutionMode mode;
        public ScriptableActionCollection actions;
    }
}
