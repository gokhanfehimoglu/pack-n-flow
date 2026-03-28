using PackNFlow.Core;

namespace PackNFlow
{
    public struct PlayflowStateChangedEvent : ISignal
    {
        public PlayflowState CurrentState;
        public PlayflowState PreviousState;
    }

    public struct ClearProgressEvent : ISignal
    {
        public float Ratio;
        public int ClearedCount;
        public int TotalCount;
    }
}
