using System.Collections.Generic;
using JetBrains.Annotations;

namespace PackNFlow.Core
{
    public static class SignalBus<T> where T : ISignal
    {
        private static readonly HashSet<ISignalBinding<T>> _subscriptions = new HashSet<ISignalBinding<T>>(16);
        private static readonly List<ISignalBinding<T>> _tempSubscriptions = new List<ISignalBinding<T>>(16);

        public static void Subscribe(SignalBinding<T> binding) => _subscriptions.Add(binding);
        public static void Unsubscribe(SignalBinding<T> binding) => _subscriptions.Remove(binding);

        public static void Publish(T signal)
        {
            _tempSubscriptions.AddRange(_subscriptions);

            int count = _tempSubscriptions.Count;
            for (int i = 0; i < count; i++)
            {
                ISignalBinding<T> binding = _tempSubscriptions[i];
                binding.OnSignal.Invoke(signal);
                binding.OnSignalNoArgs.Invoke();
            }

            _tempSubscriptions.Clear();
        }

        [UsedImplicitly]
        private static void Reset()
        {
            _subscriptions.Clear();
            _tempSubscriptions.Clear();
        }
    }
}
