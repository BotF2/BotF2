// File:ScriptExpression.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Microsoft.Scripting;

using Obtics.Values;

using Supremacy.Annotations;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Scripting.Runtime;
using Supremacy.Utility;

namespace Supremacy.Scripting
{
    [Serializable]
    public class ScriptExpression : INotifyPropertyChanged
    {
        private readonly bool _returnObservableResult;

        #region ScriptCode Property
        private string _scriptCode;

        [field: NonSerialized]
        public event EventHandler ScriptCodeChanged;

        public string ScriptCode
        {
            get { return _scriptCode; }
            set
            {
                if (value == _scriptCode)
                    return;
                _scriptCode = value;
                _delegate = null;
                OnScriptCodeChanged();
                OnPropertyChanged("ScriptCode");
            }
        }

        protected void OnScriptCodeChanged()
        {
            ScriptCodeChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Parameters Property
        private ScriptParameters _parameters;

        public ScriptParameters Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = new ScriptParameters();
                return _parameters;
            }
            set
            {
                if (value == _parameters)
                    return;
                _parameters = value;
                _delegate = null;
                OnPropertyChanged("Parameters");
            }
        }
        #endregion

        #region Script Internals
        [NonSerialized]
        private Delegate _delegate;

        public Type ReturnType
        {
            get
            {
                CompileScript();
             
                if (_delegate != null)
                    return _delegate.Method.ReturnType;
                
                return typeof(object);
            }
        }

        public ScriptExpression(bool returnObservableResult = true)
        {
            _returnObservableResult = returnObservableResult;
        }

        public TResult Evaluate<TResult>(RuntimeScriptParameters parameters)
        {
            object result = Evaluate(parameters);

            if (!typeof(IValueProvider).IsAssignableFrom(typeof(TResult)))
            {
                if (result is IValueProvider valueProvider)
                {
                    return (TResult)valueProvider.Value;
                }
            }
            
            return (TResult)result;
        }

        public object Evaluate(RuntimeScriptParameters parameters)
        {
            CompileScript();
            return _delegate.DynamicInvoke(ResolveParameterValues(parameters).ToArray());  // on break check parameters - mostly for Martial Law checking Morale value condition
        }

        public bool CompileScript([CanBeNull] ErrorSink errorSink = null)
        {
            if (_delegate != null)
            {
                return true;
            }

            ScriptService scriptService = ScriptService.Instance;// ServiceLocator.Current.GetInstance<IScriptService>();
            SourceUnit source = scriptService.Context.CreateSnippet(ScriptCode, SourceCodeKind.Expression);
            ScriptCompilerOptions options = new ScriptCompilerOptions(new ScriptParameters(Parameters));
            ErrorCounter errorCounter = (errorSink != null) ? new ErrorCounter(errorSink) : new ErrorCounter(new LogErrorSink());

            LambdaExpression lambdaExpression;

            try
            {
                lambdaExpression = scriptService.Context.ParseScript(
                    source,
                    options,
                    errorCounter);
            }
            catch (Exception)
            {
                if (!errorCounter.AnyError)
                {
                    return false;
                }

                throw;
            }

            if (errorCounter.AnyError)
            {
                return false;
            }

            _delegate = _returnObservableResult ? ExpressionObserver.Compile(lambdaExpression) : lambdaExpression.Compile();

            return true;
        }

        private IEnumerable<object> ResolveParameterValues(RuntimeScriptParameters parameters)
        {
            foreach (ScriptParameter parameter in Parameters)
            {
                if (parameters.TryGetValue(parameter, out RuntimeScriptParameter runtimeParameter))
                    yield return runtimeParameter.Value;
                else if (parameter.IsRequired)
                    throw new InvalidOperationException("Required parameter not specified: " + parameter.Name);
                else
                    yield return parameter.DefaultValue;
            }
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private sealed class LogErrorSink : ErrorSink
        {
            public override void Add(string message, string path, string code, string line, SourceSpan span, int errorCode, Severity severity)
            {
                if (severity == Severity.Error || severity == Severity.FatalError)
                    GameLog.Core.General.Error(message);
                else
                    GameLog.Core.General.Error(message);
            }

            public override void Add(SourceUnit source, string message, SourceSpan span, int errorCode, Severity severity)
            {
                if (severity == Severity.Error || severity == Severity.FatalError)
                    GameLog.Core.General.Error(message);
                else
                    GameLog.Core.General.Error(message);
            }
        }
    }
}