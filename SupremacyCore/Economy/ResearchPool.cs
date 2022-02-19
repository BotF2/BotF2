// ResearchPool.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Supremacy.Economy
{
    /// <summary>
    /// Represents a research project being undertaken by a civilization in the game.
    /// </summary>
    [Serializable]
    public class ResearchProject
    {
        private readonly int _applicationId;
        private readonly Meter _progress;

        /// <summary>
        /// Gets the progress of a <see cref="ResearchProject"/>.
        /// </summary>
        /// <value>The progress.</value>
        public Meter Progress => _progress;

        /// <summary>
        /// Gets a value indicating whether this <see cref="ResearchProject"/> is finished.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="ResearchProject"/> is finished; otherwise, <c>false</c>.
        /// </value>
        public bool IsFinished => _progress.IsMaximized;

        /// <summary>
        /// Gets the application being researched in this <see cref="ResearchProject"/>.
        /// </summary>
        /// <value>The application.</value>
        public ResearchApplication Application => GameContext.Current.ResearchMatrix.GetApplication(_applicationId);

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchProject"/> class.
        /// </summary>
        /// <param name="application">The application.</param>
        public ResearchProject(ResearchApplication application)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            _applicationId = application.ApplicationID;
            _progress = new Meter(0, 0, application.ResearchCost);
        }
    }

    /// <summary>
    /// Represents a civilization's research pool.
    /// </summary>
    [Serializable]
    public class ResearchPool : INotifyPropertyChanged
    {
        private readonly int _ownerId;
        private readonly int[] _techLevels;
        private readonly DistributionGroup<int> _distributions;
        private readonly ResearchPoolValueCollection _values;
        private readonly ResearchBonusCollection _bonuses;
        //private readonly ResearchPointsCollection _points;

        private readonly List<ResearchProject>[] _queue;
        private readonly Meter _cumulativePoints;

        /// <summary>
        /// Gets the owner of this <see cref="ResearchPool"/>.
        /// </summary>
        /// <value>The owner.</value>
        public Civilization Owner => GameContext.Current.Civilizations[_ownerId];

        /// <summary>
        /// Gets the cumulative number of research points generated by the owner.
        /// </summary>
        /// <value>The number cumulative research points generated.</value>
        public Meter CumulativePoints => _cumulativePoints;

        /// <summary>
        /// Gets the distribution of research points between the various fields.
        /// </summary>
        /// <value>The distribution of points between fields.</value>
        public DistributionGroup<int> Distributions => _distributions;

        /// <summary>
        /// Gets the research points being allocated to each field for the current turn
        /// </summary>
        /// <value>The current research point allocations.</value>
        public ResearchPoolValueCollection Values => _values;

        /// <summary>
        /// Gets the current bonuses available for each research category.
        /// </summary>
        /// <value>The bonuses for each category.</value>
        public ResearchBonusCollection Bonuses => _bonuses;

        /// <summary>
        /// Gets the current bonuses available for each research category.
        /// </summary>
        /// <value>The bonuses for each category.</value>
        //public ResearchPointsCollection Points
        //{
        //    get { return _points; }
        //}

        /// <summary>
        /// Determines whether the specified application has been researched.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <returns>
        /// <c>true</c> if the specified application has been researched; otherwise, <c>false</c>.
        /// </returns>
        public bool IsResearched(ResearchApplication application)
        {
            foreach (ResearchField field in GameContext.Current.ResearchMatrix.Fields)
            {
                if (field.Applications.Contains(application))
                {
                    foreach (ResearchProject project in _queue[field.FieldID])
                    {
                        if (project.Application == application)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified application is currently being researched.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <returns>
        /// <c>true</c> if the application is being researched; otherwise, <c>false</c>.
        /// </returns>
        public bool IsResearching(ResearchApplication application)
        {
            foreach (ResearchField field in GameContext.Current.ResearchMatrix.Fields)
            {
                if (field.Applications.Contains(application))
                {
                    ResearchProject project = GetCurrentProject(field);
                    return (project != null) && (project.Application == application);
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the current tech level of the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The tech level.</returns>
        public int GetTechLevel(ResearchField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            return GetTechLevel(field.FieldID);
        }

        /// <summary>
        /// Gets the current tech level of the specified tech category.
        /// </summary>
        /// <param name="category">The tech category.</param>
        /// <returns>The tech level.</returns>
        public int GetTechLevel(TechCategory category)
        {
            List<int> levels = new List<int>();
            foreach (ResearchField field in GameContext.Current.ResearchMatrix.Fields.Where(o => o.TechCategory == category))
            {
                levels.Add(GetTechLevel(field));
            }
            if (levels.Count == 0)
            {
                return 0;
            }

            return levels.Min();
        }

        /// <summary>
        /// Gets the current tech level of the specified field.
        /// </summary>
        /// <param name="fieldId">The field ID.</param>
        /// <returns>The tech level.</returns>
        public int GetTechLevel(int fieldId)
        {
            return _techLevels[fieldId];
        }


        /// <summary>
        /// Gets the next application to be researched in the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The next application.</returns>
        public ResearchApplication GetNextApplication(ResearchField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            if (_queue[field.FieldID].Count == 0)
            {
                return null;
            }

            return _queue[field.FieldID][0].Application;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchPool"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="matrix">The research matrix.</param>
        public ResearchPool(Civilization owner, ResearchMatrix matrix)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            List<int> fieldIds = new List<int>();

            _ownerId = owner.CivID;
            _values = new ResearchPoolValueCollection();
            _bonuses = new ResearchBonusCollection(owner);
            //_points = new ResearchPointsCollection(owner);
            _techLevels = new int[matrix.Fields.Count];
            _queue = new List<ResearchProject>[matrix.Fields.Count];
            _cumulativePoints = new Meter(0, int.MaxValue);

            Data.Table startingTechLevelsTable = GameContext.Current.Tables.GameOptionTables["StartingTechLevels"];
            StartingTechLevel startingTechLevel = GameContext.Current.Options.StartingTechLevel;

            Dictionary<TechCategory, int> initialFieldLevelValues = null;

            bool ownerIsEmpire = owner.IsEmpire;
            if (ownerIsEmpire)
            {
                initialFieldLevelValues = EnumHelper.GetValues<TechCategory>().ToDictionary(
                    techCategory => techCategory,
                    techCategory =>
                    Number.ParseInt32(startingTechLevelsTable[startingTechLevel.ToString()][techCategory.ToString()]));
            }

            foreach (ResearchField field in matrix.Fields)
            {
                fieldIds.Add(field.FieldID);
                _techLevels[field.FieldID] = ownerIsEmpire ? initialFieldLevelValues[field.TechCategory] : 1;
                _queue[field.FieldID] = new List<ResearchProject>();
                foreach (ResearchApplication application in field.Applications)
                {
                    if (application.Level > GetTechLevel(field))
                    {
                        _queue[field.FieldID].Add(new ResearchProject(application));
                    }
                    else
                    {
                        _cumulativePoints.BaseValue += application.ResearchCost;

                        // Make sure the current and last values match so to not show any deltas
                        _cumulativePoints.Reset();
                        _cumulativePoints.SaveCurrentAndResetToBase();
                    }
                }
            }

            _distributions = new DistributionGroup<int>(fieldIds);
            _distributions.DistributeEvenly();
        }

        /// <summary>
        /// Gets the project currently being researched in the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The current project.</returns>
        public ResearchProject GetCurrentProject(ResearchField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            return GetCurrentProject(field.FieldID);
        }

        /// <summary>
        /// Gets the project currently being researched in the specified field.
        /// </summary>
        /// <param name="fieldId">The field ID.</param>
        /// <returns>The current project.</returns>
        public ResearchProject GetCurrentProject(int fieldId)
        {
            if (_queue[fieldId].Count == 0)
            {
                return null;
            }

            return _queue[fieldId][0];
        }

        /// <summary>
        /// Applies the specified number of research points to the currently active projects.
        /// </summary>
        /// <param name="researchPoints">The number of research points.</param>
        public void UpdateResearch(int researchPoints)
        {
            //GameLog.Client.ResearchDetails.InfoFormat("UpdatingResearch...");

            if (researchPoints < 0)
            {
                researchPoints = 0;
            }

            _distributions.TotalValue = researchPoints;

            List<string> _alreadyDone = new List<string>();

            string researchSummary = "";
            string distributionSummary = "";
            CivilizationManager civManager = GameContext.Current.CivilizationManagers[_ownerId];

            researchSummary += "Progress: ";// + "Gained P. = " + researchPoints + " Progress: "; 
            distributionSummary += "Research Distrib. ";

            foreach (ResearchField field in GameContext.Current.ResearchMatrix.Fields)
            {
                int fieldPoints = _distributions.Values[field.FieldID];

                // works but gives no good overview yet
                //GameLog.Print("BORG only: Total ResearchPoints before {4} plus current {0} - {3} to {1}, plus Bonus ({2} * {3})",
                //    researchPoints, field.TechCategory, _bonuses[field.TechCategory], fieldPoints, _cumulativePoints);

                fieldPoints += (int)(_bonuses[field.TechCategory] * fieldPoints);
                _ = _cumulativePoints.AdjustCurrent(fieldPoints);



                for (int i = 0; i < _queue[field.FieldID].Count; i++)
                {

                    researchSummary += " - " + field.TechCategory + "-" + _queue[field.FieldID][i].Application.Level + ": " + _queue[field.FieldID][i].Progress.PercentFilled;

                    //civManager.SitRepEntries.Add(new ScienceSummarySitRepEntry(Owner, researchSummary));


                    if (_queue[field.FieldID][i].IsFinished)
                    {
                        FinishProject(field.FieldID, i--);
                        continue;
                    }
                    fieldPoints -= _queue[field.FieldID][i].Progress.AdjustCurrent(fieldPoints);

                    if (_queue[field.FieldID][i].IsFinished)
                    {
                        FinishProject(field.FieldID, i--);
                        continue;
                    }
                    if (fieldPoints <= 0)
                    {
                        break;
                    }
                }

            }

            distributionSummary += " - Bio " + _distributions[0].Value.ToString()/* + ", "*/
                    + " - Comp. " + _distributions[1].Value.ToString()/* + ", "*/
                    + " - Constr. " + _distributions[2].Value.ToString()/* + ", "*/
                    + " - Energy " + _distributions[3].Value.ToString()/* + ", "*/
                    + " - Prop. " + _distributions[4].Value.ToString()/* + ", "*/
                    + " - Weapon " + _distributions[5].Value.ToString()/* + ", "*/
                    ;
            //distributionSummary += "- Gained = " + researchPoints;

            //if (researchPoints > 100)  // don't do it for Science Ships gaining 20,40 
            //{
            //civManager.SitRepEntries.Add(new ScienceSummarySitRepEntry(Owner, distributionSummary));  // Percentage each field
            //civManager.SitRepEntries.Add(new ScienceSummarySitRepEntry(Owner, researchSummary));  // Points each field

            if(!_alreadyDone.Contains(civManager.Civilization.CivID + "-" + GameContext.Current.TurnNumber))
            { 
                civManager.SitRepEntries.Add(new Report_NoAction(Owner, distributionSummary, "", "", SitRepPriority.Gray)); // Percentage each field
                civManager.SitRepEntries.Add(new Report_NoAction(Owner, researchSummary, "", "", SitRepPriority.Purple));  // Points each field
            }

            _alreadyDone.Add(civManager.Civilization.CivID + "-" + GameContext.Current.TurnNumber);

            _cumulativePoints.UpdateAndReset();
            //GameLog.Client.ResearchDetails.InfoFormat("UpdatingResearch...DONE");
        }

        /// <summary>
        /// Finishes the project located at the specified index in the queue for a given field.
        /// </summary>
        /// <param name="fieldId">The field id.</param>
        /// <param name="queueIndex">The index in the queue.</param>
        private void FinishProject(int fieldId, int queueIndex)
        {
            ResearchApplication finishedApp = _queue[fieldId][queueIndex].Application;
            CivilizationManager civManager = GameContext.Current.CivilizationManagers[Owner];
            ICollection<TechObjectDesign> designsBefore = TechTreeHelper.GetDesignsForCurrentTechLevels(Owner);

            _queue[fieldId].RemoveAt(queueIndex);
            UpdateTechLevels();

            ICollection<TechObjectDesign> designsAfter = TechTreeHelper.GetDesignsForCurrentTechLevels(Owner);
            List<TechObjectDesign> newDesigns = designsAfter.Except(designsBefore).ToList();

            if (civManager != null)
            {
                civManager.SitRepEntries.Add(new ResearchCompleteSitRepEntry(Owner, finishedApp, newDesigns));
            }
        }

        /// <summary>
        /// Updates the current tech levels based on new research completed.
        /// </summary>
        protected void UpdateTechLevels()
        {
            foreach (ResearchField field in GameContext.Current.ResearchMatrix.Fields)
            {
                int nextTechLevel = _techLevels[field.FieldID];

                if (GetCurrentProject(field) != null)
                {
                    nextTechLevel = GetCurrentProject(field).Application.Level;
                }

                if (nextTechLevel > _techLevels[field.FieldID])
                {
                    _techLevels[field.FieldID] = nextTechLevel - 1;
                }
            }
        }

        internal void RefreshBonuses()
        {
            OnPropertyChanged("Bonuses");
        }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// A collection of <see cref="Meter"/>s indexed by <see cref="TechCategory"/>.
    /// </summary>
    [Serializable]
    public class ResearchPoolValueCollection
        : Dictionary<TechCategory, Meter>,
          IOwnedDataSerializableAndRecreatable,
          ICloneable
    {

        public ResearchPoolValueCollection()
        {
            _ = EnumHelper.GetValues<TechCategory>().ForEach(t => Add(t, new Meter()));
        }

        public ResearchPoolValueCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public ResearchPoolValueCollection Clone()
        {
            ResearchPoolValueCollection clone = new ResearchPoolValueCollection();
            foreach (KeyValuePair<TechCategory, Meter> entry in this)
            {
                clone.Add(entry.Key, entry.Value.Clone());
            }
            return clone;
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(this);
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            Dictionary<TechCategory, Meter> data = reader.ReadDictionary<TechCategory, Meter>();
            _ = EnumHelper.GetValues<TechCategory>().ForEach(r => this[r] = data[r].Clone());
        }
    }

    /// <summary>
    /// A collection of percentage-based research bonuses indexed by <see cref="TechCategory"/>
    /// and research field ID.
    /// </summary>
    [Serializable]
    public class ResearchBonusCollection
    {
        private readonly int _ownerId;
        public readonly string _text;

        /// <summary>
        /// Gets or sets the percentage-based bonus for the specified field.
        /// </summary>
        /// <value>The bonus.</value>
        public Percentage this[TechCategory field]
        {
            get
            {
                CivilizationManager civManager = GameContext.Current.CivilizationManagers[_ownerId];
                switch (field)
                {
                    case TechCategory.BioTech:
                        return civManager.GlobalBonuses
                            .Where(o => (o.BonusType == BonusType.PercentBioTechResearch) || (o.BonusType == BonusType.PercentResearchEmpireWide))
                            .Sum(o => 0.01f * o.Amount);
                    case TechCategory.Computers:
                        return civManager.GlobalBonuses
                            .Where(o => (o.BonusType == BonusType.PercentComputerResearch) || (o.BonusType == BonusType.PercentResearchEmpireWide))
                            .Sum(o => 0.01f * o.Amount);
                    case TechCategory.Construction:
                        return civManager.GlobalBonuses
                            .Where(o => (o.BonusType == BonusType.PercentConstructionResearch) || (o.BonusType == BonusType.PercentResearchEmpireWide))
                            .Sum(o => 0.01f * o.Amount);
                    case TechCategory.Energy:
                        return civManager.GlobalBonuses
                            .Where(o => (o.BonusType == BonusType.PercentEnergyResearch) || (o.BonusType == BonusType.PercentResearchEmpireWide))
                            .Sum(o => 0.01f * o.Amount);
                    case TechCategory.Propulsion:
                        return civManager.GlobalBonuses
                            .Where(o => (o.BonusType == BonusType.PercentPropulsionResearch) || (o.BonusType == BonusType.PercentResearchEmpireWide))
                            .Sum(o => 0.01f * o.Amount);
                    case TechCategory.Weapons:
                        return civManager.GlobalBonuses
                            .Where(o => (o.BonusType == BonusType.PercentWeaponsResearch) || (o.BonusType == BonusType.PercentResearchEmpireWide))
                            .Sum(o => 0.01f * o.Amount);
                }
                return 0.0f;
            }
        }

        /// <summary>
        /// Gets or sets the percentage-based bonus for the specified field ID.
        /// </summary>
        /// <value>The bonus.</value>
        public Percentage this[int fieldId]
        {
            get
            {
                //_text = this[GameContext.Current.ResearchMatrix.Fields[fieldId].TechCategory] + " percent DONE of " + GameContext.Current.ResearchMatrix.Fields[fieldId].TechCategory;
                //Console.WriteLine(_text);
                //GameLog.Core.ResearchDetails.DebugFormat(_text);

                return this[GameContext.Current.ResearchMatrix.Fields[fieldId].TechCategory];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchBonusCollection"/> class.
        /// </summary>
        public ResearchBonusCollection([NotNull] Civilization owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            // works    GameLog.Print("ResearchBonusCollection Owner = {0} = {1}", owner.CivID, owner.Name);
            _ownerId = owner.CivID;
        }
    }
}
