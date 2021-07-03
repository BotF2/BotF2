// EffectSystem.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

using Obtics.Values;

using Supremacy.Annotations;
using Supremacy.Types;

using System.Linq;

using Supremacy.Collections;
using Supremacy.Utility;

namespace Supremacy.Effects
{
    public static class EffectSystem
    {
        private static readonly StateScope SuspendEffectsScope;
        private static readonly Dictionary<IEffectSource, EffectGroupWorker[]> RegisteredSources;
        private static CompositeDisposer SuspendedEffectsHandle;

        public static object SyncRoot { get; private set; }

        static EffectSystem()
        {
            SyncRoot = new object();
            SuspendEffectsScope = new StateScope(TryResumeEffects);
            RegisteredSources = new Dictionary<IEffectSource, EffectGroupWorker[]>();
        }

        public static bool IsEnabled { get; private set; }

        private static void TryResumeEffects()
        {
            lock (SyncRoot)
            {
                if (SuspendEffectsScope.IsWithin)
                {
                    IEnumerable<IDisposable> suspendHandles = RegisteredSources
                        .SelectMany(o => o.Key.EffectGroupBindings)
                        .SelectMany(g => g.EffectBindings)
                        .Select(e => e.Suspend());
                    SuspendedEffectsHandle = new CompositeDisposer(suspendHandles);
                }
                else
                {
                    CompositeDisposer disposer = Interlocked.Exchange(ref SuspendedEffectsHandle, null);
                    if (disposer != null)
                    {
                        disposer.Dispose();
                    }
                }

            }
        }

        public static void RegisterEffectSource([NotNull] IEffectSource effectSource)
        {
            if (effectSource == null)
            {
                throw new ArgumentNullException("effectSource");
            }

            lock (SyncRoot)
            {
                if (RegisteredSources.ContainsKey(effectSource))
                {
                    return;
                }

                IEffectGroupBindingCollection effectGroups = effectSource.EffectGroupBindings;

                if (SuspendEffectsScope.IsWithin)
                {
                    _ = effectGroups
                        .SelectMany(o => o.EffectBindings)
                        .Select(o => o.Suspend())
                        .ForEach(o => SuspendedEffectsHandle.AddChild(o));
                }

                EffectGroupWorker[] workers = new EffectGroupWorker[effectGroups.Count];

                for (int i = 0; i < workers.Length; i++)
                {
                    workers[i] = new EffectGroupWorker(effectSource, effectGroups[i]);
                    if (IsEnabled)
                    {
                        workers[i].Activate();
                    }
                }

                RegisteredSources.Add(effectSource, workers);
            }
        }

        public static void UnregisterEffectSource([NotNull] IEffectSource effectSource)
        {
            if (effectSource == null)
            {
                throw new ArgumentNullException("effectSource");
            }

            lock (SyncRoot)
            {

                if (!RegisteredSources.TryGetValue(effectSource, out EffectGroupWorker[] workers))
                {
                    return;
                }

                _ = RegisteredSources.Remove(effectSource);

                if (!IsEnabled)
                {
                    return;
                }

                foreach (EffectGroupWorker effectGroupWorker in workers)
                {
                    effectGroupWorker.Deactivate();
                }
            }
        }

        public static void EnableEffects()
        {
            lock (SyncRoot)
            {
                if (IsEnabled)
                {
                    return;
                }

                try
                {
                    _ = RegisteredSources.SelectMany(o => o.Value).ForEach(o => o.Activate());
                }
                finally
                {
                    IsEnabled = true;
                }
            }
        }

        public static void DisableEffects()
        {
            lock (SyncRoot)
            {
                if (!IsEnabled)
                {
                    return;
                }

                try
                {
                    _ = RegisteredSources.SelectMany(o => o.Value).ForEach(o => o.Deactivate());
                }
                finally
                {
                    IsEnabled = false;
                }
            }
        }

        public static bool IsSuspended => SuspendEffectsScope.IsWithin;

        public static IDisposable SuspendEffects()
        {
            return SuspendEffectsScope.Enter();
        }

        private class EffectGroupWorker
        {
            private readonly IEffectSource _source;
            private readonly EffectGroupBinding _effectGroupBinding;
            private readonly HashSet<object> _attachedTargets;

            private IValueProvider<bool> _activationTest;
            private IValueProvider _scope;
            private object _lastScopeValue;
            private bool _isActive;
            private bool _isObservingScope;

            internal EffectGroupWorker([NotNull] IEffectSource source, [NotNull] EffectGroupBinding effectGroupBinding)
            {
                _source = source ?? throw new ArgumentNullException("source");
                _effectGroupBinding = effectGroupBinding ?? throw new ArgumentNullException("effectGroupBinding");
                _attachedTargets = new HashSet<object>();
            }

            private void SetScopeObservation(object scope, bool enableObservation)
            {
                if (!(scope is INotifyCollectionChanged observableScope))
                {
                    return;
                }

                if (enableObservation && !_isObservingScope)
                {
                    observableScope.CollectionChanged += OnScopeChanged;
                }
                else if (!enableObservation && _isObservingScope)
                {
                    observableScope.CollectionChanged -= OnScopeChanged;
                }

                _isObservingScope = enableObservation;
            }

            internal void Activate()
            {
                if (_isActive)
                {
                    return;
                }

                try
                {
                    Scripting.RuntimeScriptParameters runtimeScriptParameters = _effectGroupBinding.BindActivationScriptRuntimeParameters();

                    _activationTest = _effectGroupBinding.EffectGroup.ActivationScript.Evaluate<IValueProvider<bool>>(runtimeScriptParameters);
                    _scope = _effectGroupBinding.EffectGroup.ScopeScript.Evaluate<IValueProvider>(runtimeScriptParameters);

                    ((INotifyPropertyChanged)_activationTest).PropertyChanged += OnActivationTestChanged;
                    ((INotifyPropertyChanged)_scope).PropertyChanged += OnScopeValueChanged;

                    ChangeScope();
                }
                finally
                {
                    _isActive = true;
                }
            }

            private void ChangeScope()
            {
                object oldScope = _lastScopeValue;
                if (oldScope != null)
                {
                    SetScopeObservation(oldScope, false);
                }

                object scopeValue = _lastScopeValue = _scope.Value;

                List<object> oldItems = _attachedTargets.ToList();
                IEnumerable newItems = scopeValue as IEnumerable ??
                               ((scopeValue == null) ? null : new[] { scopeValue });
                if (_isActive)
                {
                    SetScopeObservation(_scope.Value, true);
                }

                UpdateTargets(oldItems, newItems);
            }

            private void OnScopeValueChanged(object sender, PropertyChangedEventArgs e)
            {
                ChangeScope();
            }

            private void UpdateTargets(IEnumerable oldItems, IEnumerable newItems)
            {
                if (oldItems != null)
                {
                    foreach (IEffectTarget oldItem in oldItems.OfType<IEffectTarget>())
                    {
                        _effectGroupBinding.DetachTarget(oldItem);
                        _ = _attachedTargets.Remove(oldItem);
                    }
                }

                if (newItems != null)
                {
                    foreach (IEffectTarget newItem in newItems.OfType<IEffectTarget>())
                    {
                        _effectGroupBinding.AttachTarget(newItem);
                        _ = _attachedTargets.Add(newItem);
                    }
                }
            }

            private void OnScopeChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                IEnumerable oldItems;
                IEnumerable newItems;

                IEnumerable scope = (IEnumerable)sender;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        {
                            newItems = e.NewItems;
                            oldItems = e.OldItems;
                            break;
                        }

                    case NotifyCollectionChangedAction.Reset:
                        {
                            HashSet<object> currentItems = scope.OfType<object>().ToHashSet();

                            currentItems.ExceptWith(_attachedTargets);
                            newItems = currentItems;

                            currentItems = scope.OfType<object>().ToHashSet();
                            _attachedTargets.ExceptWith(currentItems);
                            oldItems = _attachedTargets;

                            _attachedTargets.Clear();

                            break;
                        }

                    default:
                    case NotifyCollectionChangedAction.Move:
                        return;
                }

                UpdateTargets(oldItems, newItems);
            }

            internal void Deactivate()
            {
                if (!_isActive)
                {
                    return;
                }

                try
                {
                    SetScopeObservation(_scope.Value, false);
                    ((INotifyPropertyChanged)_activationTest).PropertyChanged -= OnActivationTestChanged;

                    UpdateTargets(_attachedTargets.ToList(), null);

                    _activationTest = null;
                    _scope = null;
                    _lastScopeValue = null;

                }
                finally
                {
                    _isActive = false;
                }
            }

            private void OnActivationTestChanged(object sender, PropertyChangedEventArgs e)
            {
                lock (SyncRoot)
                {
                    if (_activationTest.Value)
                    {
                        GameLog.Core.General.DebugFormat(
                            "Effect condition activated for source object {{{0}}}: {1}",
                            _source,
                            _effectGroupBinding.EffectGroup.ActivationDescription);
                        UpdateTargets(null, _scope.Value as IEnumerable ?? new[] { _scope.Value });
                        SetScopeObservation(_scope.Value, true);
                    }
                    else
                    {
                        GameLog.Core.General.DebugFormat(
                            "Effect condition deactivated for source object {{{0}}}: {1}",
                            _source,
                            _effectGroupBinding.EffectGroup.ActivationDescription);

                        UpdateTargets(_scope.Value as IEnumerable ?? new[] { _scope.Value }, null);
                        SetScopeObservation(_scope.Value, false);
                    }
                }
            }
        }
    }
}