// CompositeCommandArgs.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Client.Commands
{
    public class CompositeCommandArgs
    {
        public bool Handled { get; set; }
    }

    public class CompositeCommandArgs<TParameter> : CompositeCommandArgs
    {
        public CompositeCommandArgs(TParameter parameter)
        {
            Parameter = parameter;
        }

        public TParameter Parameter { get; }
    }
}