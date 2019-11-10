// ScriptedEventSitRepEntryData.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Linq.Expressions;

using Supremacy.Annotations;
using Supremacy.Entities;

namespace Supremacy.Scripting
{
    public class ScriptedEventSitRepEntryData
    {
        public ScriptedEventSitRepEntryData(
            [NotNull] Civilization owner,
            [NotNull] string headerText,
            [NotNull] string summaryText,
            [NotNull] string detailText,
            params Expression<Func<object>>[] detailTextParameterResolvers)
            : this(owner, headerText, summaryText, detailText, null, null, detailTextParameterResolvers) { }

        public ScriptedEventSitRepEntryData(
            [NotNull] Civilization owner,
            [NotNull] string headerText,
            [NotNull] string summaryText,
            [NotNull] string detailText,
            [CanBeNull] string detailImage,
            params Expression<Func<object>>[] detailTextParameterResolvers)
            : this(owner, headerText, summaryText, detailText, detailImage, null, detailTextParameterResolvers) { }

        public ScriptedEventSitRepEntryData(
            [NotNull] Civilization owner,
            [NotNull] string headerText,
            [NotNull] string summaryText,
            [NotNull] string detailText,
            [CanBeNull] string detailImage,
            [CanBeNull] string soundEffect,
            params Expression<Func<object>>[] detailTextParameterResolvers)
        {
            Owner = owner;
            HeaderText = headerText;
            SummaryText = summaryText;
            DetailText = detailText;
            DetailImage = detailImage;
            SoundEffect = soundEffect;
            DetailTextParameterResolvers = detailTextParameterResolvers;
        }

        [NotNull]
        public Civilization Owner { get; private set; }

        [NotNull]
        public string HeaderText { get; private set; }

        [NotNull]
        public string SummaryText { get; private set; }

        [NotNull]
        public string DetailText { get; private set; }

        [CanBeNull]
        public string DetailImage { get; private set; }

        [CanBeNull]
        public string SoundEffect { get; private set; }

        [NotNull]
        public Expression<Func<object>>[] DetailTextParameterResolvers { get; private set; }
    }
}