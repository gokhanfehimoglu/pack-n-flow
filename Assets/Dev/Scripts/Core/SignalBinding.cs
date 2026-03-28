using System;

namespace PackNFlow.Core
{
    public class SignalBinding<T> : ISignalBinding<T> where T : ISignal
    {
        private Action<T> _onSignal = _ => { };
        private Action _onSignalNoArgs = () => { };

        Action<T> ISignalBinding<T>.OnSignal => _onSignal;
        Action ISignalBinding<T>.OnSignalNoArgs => _onSignalNoArgs;

        public SignalBinding(Action<T> onSignal) => this._onSignal = onSignal;
        public SignalBinding(Action onSignalNoArgs) => this._onSignalNoArgs = onSignalNoArgs;

        public void Add(Action<T> onSignal) => this._onSignal += onSignal;
        public void Remove(Action<T> onSignal) => this._onSignal -= onSignal;

        public void Add(Action onSignalNoArgs) => this._onSignalNoArgs += onSignalNoArgs;
        public void Remove(Action onSignalNoArgs) => this._onSignalNoArgs -= onSignalNoArgs;
    }

    internal interface ISignalBinding<T>
    {
        public Action<T> OnSignal { get; }
        public Action OnSignalNoArgs { get; }
    }
}
