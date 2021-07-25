// File:SitRepEntry.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Scripting;
using Supremacy.Tech;
using Supremacy.Text;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Supremacy.Game
{
    /// <summary>
    /// Serverity of a situation report entry
    /// </summary>
    public enum SitRepPriority
    {
        /// <summary>
        /// A green situation report entry reflects a normal or informal status message.
        /// </summary>
        Green,
        /// <summary>
        /// A yellow situation report entry reflects a status message, where the player should consider to react.
        /// </summary>
        Orange,
        /// <summary>
        /// A red siutation report entry reflects a urgend status message. The play must react.
        /// </summary>
        Red,
        /// <summary>
        /// A special event, like a battle, or an event.
        /// </summary>
        Blue,
        /// <summary>
        /// A special event, like a battle, or an event.
        /// </summary>
        Gray,
        /// <summary>
        /// A special event, like a battle, or an event.
        /// </summary>
        Purple,
        /// <summary>
        /// Shutdowns due to Energy or something else
        /// </summary>
        Pink,
        /// <summary>
        /// Shutdowns due to Energy or something else
        /// </summary>
        Aqua,
        /// <summary>
        /// Shutdowns due to Energy or something else
        /// </summary>
        Brown,
        /// <summary>
        /// Shutdowns due to Energy or something else
        /// </summary>
        Golden,
        /// <summary>
        /// Shutdowns due to Energy or something else
        /// </summary>
        Crimson,
        /// <summary>
        /// Shutdowns due to Energy or something else
        /// </summary>
        RedYellow,
        /// <summary>
        /// Shutdowns due to Energy or something else
        /// </summary>
        BlueDark,

        Yellow // Yellow is needed for old Saved Games
    }

    public enum SitRepDone
    {
        /// <summary>
        /// A green situation report entry reflects a normal or informal status message.
        /// </summary>
        Unread,
        /// <summary>
        /// A yellow situation report entry reflects a status message, where the player should consider to react.
        /// </summary>
        Read,
        /// <summary>
        /// A red siutation report entry reflects a urgend status message. The play must react.
        /// </summary>
        Done,
        /// <summary>
        /// A special event, like a battle, or an event.
        /// </summary>
        Ignore
    }

    [Flags]
    public enum SitRepCategory
    {
        General = 0x00000001,
        NewColony = 0x00000002,
        ColonyStatus = 0x00000004,
        Construction = 0x00000008,
        Resources = 0x00000010,
        Diplomacy = 0x00000020,
        Military = 0x00000040,
        Research = 0x00000080,
        Intelligence = 0x00000100,
        NewInfiltrate = 0x00000200,
        SpecialEvent = 0x00000400,
        FirstContact = 0x00000800,
    }

    public enum SitRepComment
    {
        open,
        DONE
    }

    public enum SitRepAction
    {
        None,
        ViewColony,
        CenterOnSector,
        ShowScienceScreen,
        ShowDiploScreen,
        ShowIntelScreen,
        SelectTaskForce
    }

    /// <summary>
    /// Base class for all SitRep entries.
    /// </summary>
    [Serializable]
    public abstract class SitRepEntry
    {
        protected readonly int _ownerId;
        protected SitRepPriority _priority;
        protected string _sitRepComment;

        /// <summary>
        /// Initializes a new instance of the <see cref="SitRepEntry"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="priority">The priority.</param>
        protected SitRepEntry(Civilization owner, SitRepPriority priority)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            _ownerId = owner.CivID;
            _priority = priority;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SitRepEntry"/> class.
        /// </summary>
        /// <param name="ownerId">The owner ID.</param>
        /// <param name="priority">The priority.</param>
        protected SitRepEntry(int ownerId, SitRepPriority priority)
        {
            if (ownerId == -1)
            {
                throw new ArgumentException("invalid Civilization ID", "ownerId");
            }

            _ownerId = ownerId;
            _priority = priority;
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        /// <value>The owner.</value>
        public Civilization Owner => GameContext.Current.Civilizations[_ownerId];

        /// <summary>
        /// Whether when double clicked in SitRepDialog, any action is to be performed
        /// </summary>
        public virtual SitRepAction Action => SitRepAction.None;
        /// <summary>
        /// The target upon which Action should act upon.
        /// </summary>
        public virtual object ActionTarget { get; }

        public abstract SitRepCategory Categories { get; }

        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public SitRepPriority Priority => _priority;

        /// <summary>
        /// Gets the summary text.
        /// </summary>
        /// <value>The summary text.</value>
        public abstract string SummaryText { get; }

        /// <summary>
        /// Gets the summary text.
        /// </summary>
        /// <value>The summary text.</value>

        public abstract string SitRepComment { get /*{ return _sitRepComment*/; set; }
        //public virtual string SitRepComment { get; set; }

        private static void SitRepComment_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(e.Source is TextBox SitRepCommentText))
            {
                return;
            }

            System.Windows.Data.BindingExpression bindingExpression = SitRepCommentText.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(SitRepCommentText.Text))
            {
                bindingExpression.UpdateSource();
            }
        }

        /// <summary>
        /// Gets the header text.
        /// </summary>
        /// <value>The header text.</value>
        public virtual string HeaderText => SummaryText;

        /// <summary>
        /// Gets a value indicating whether this <see cref="SitRepEntry"/> has detailed text.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="SitRepEntry"/> has detailed text; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasDetails => !string.IsNullOrEmpty(DetailText);

        public bool HasSoundEffect => SoundEffect != null;

        public virtual string SoundEffect => null;

        public virtual string SummaryImage => null;



        /// <summary>
        /// Gets the detail text.
        /// </summary>
        /// <value>The detail text.</value>
        public virtual string DetailText => null;

        /// <summary>
        /// Gets the detail image path.
        /// </summary>
        /// <value>The detail image path.</value>
        public virtual string DetailImage => null;

        /// <summary>
        /// Gets a value indicating whether this <see cref="SitRepEntry"/> is a priority entry.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="SitRepEntry"/> is a priority entry; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsPriority => false;

    }

    [Serializable]
    public class ReportEntryCoS : SitRepEntry  // Centers on Sector
    {
        //private readonly int _colonyID;
        private readonly MapLocation _loc;
        private readonly string _comment;
        private readonly string _report;
        private readonly SitRepPriority _priority;


        public ReportEntryCoS(Civilization owner, MapLocation loc, string report, string comment, SitRepPriority priority)
            : base(owner, priority)
        {
            if (loc == null)
            {
                throw new ArgumentNullException("loc");
            }

            _loc = loc;
            _comment = comment;
            _report = report;
            _priority = priority;

        }
        //public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        //public override SitRepAction Action => SitRepAction.ViewColony;
        //public override object ActionTarget => Colony;

        public override SitRepCategory Categories => SitRepCategory.General;
        //public override SitRepPriority Priority => _priority;

        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
        public override bool HasDetails => false; // turn on/off for extra Dialog window
        //public override string HeaderText => string.Format(ResourceManager.GetString("ASTEROID_IMPACT_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => _report;
        public override string SummaryText => _report;
        public override string SitRepComment { get => _comment; set { } }
        //public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/AsteroidImpact.png";
        public override bool IsPriority => true;
    }

    [Serializable]
    public class AsteroidImpactSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public AsteroidImpactSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Pink)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("ASTEROID_IMPACT_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("ASTEROID_IMPACT_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string SummaryText => string.Format(ResourceManager.GetString("ASTEROID_IMPACT_SUMMARY_TEXT"), Colony.Name, Colony.Location);
        public override string SitRepComment { get; set; }
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/AsteroidImpact.png";
        public override bool IsPriority => true;
    }

    [Serializable]
    public class BlackHoleEncounterSitRepEntry : SitRepEntry
    {
        private readonly MapLocation _location;
        private readonly int _shipsDestroyed;
        private readonly int _shipsDamaged;
        public BlackHoleEncounterSitRepEntry(Civilization owner, MapLocation location, int shipsDamaged, int shipsDestroyed)
            : base(owner, SitRepPriority.Gray)
        {
            _location = location;
            _shipsDamaged = shipsDamaged;
            _shipsDestroyed = shipsDestroyed;
        }
        public override SitRepCategory Categories => SitRepCategory.General;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_location];
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_BLACK_HOLE_ENCOUNTER"), _location, _shipsDestroyed, _shipsDamaged);
        public override string SitRepComment { get; set; }
        public override bool IsPriority => true;

    }

    [Serializable]
    public class BuildingBuiltSitRepEntry : ItemBuiltSitRepEntry
    {
        private readonly bool _isActive;
        private readonly int _colonyID;

        public BuildingBuiltSitRepEntry(Civilization owner, TechObjectDesign itemType, MapLocation location, bool isActive)
        : base(owner, itemType, location, SitRepPriority.RedYellow)
        {
            _colonyID = GameContext.Current.Universe.Map[Location].System.Colony.ObjectID;
            _isActive = isActive;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_CONSTRUCTED_UNPOWERED"),
                    GameContext.Current.Universe.Map[Location].Name,
                    GameContext.Current.Universe.Map[Location].Location,
                    ResourceManager.GetString(ItemType.Name),
                    _isActive ? "" : " ("
                    + string.Format(ResourceManager.GetString("UN"))
                    + string.Format(ResourceManager.GetString("POWERED"))
                    + ")");
    }

    [Serializable]
    public class BuildProjectStatusSitRepEntry : SitRepEntry
    {
        private readonly string _note;
        private readonly MapLocation _loc;

        public BuildProjectStatusSitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Gray)
        { _loc = loc; _note = Note; }

        public string Note => _note;
        public override SitRepCategory Categories => SitRepCategory.Construction;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _note;
    }
    // End of SitRepEntry

    [Serializable]
    public class BuildProjectResourceShortageSitRepEntry : SitRepEntry
    {
        //private readonly bool _isActive;
        //private readonly int _colonyID;
        private readonly string _delta;
        private readonly string _project;
        private readonly string _resource;

        public BuildProjectResourceShortageSitRepEntry(Civilization owner, string resource, string delta, string project)
            : base(owner, SitRepPriority.Red)
        {
            //_colonyID = GameContext.Current.Universe.Map[Location].System.Colony.ObjectID;

            _resource = resource;
            _delta = delta;
            _project = project;
            //var sitrepText = "Not able to finish project " + _project + ", due to missing " + _delta + " " + _resource;

        }
        public override SitRepCategory Categories => SitRepCategory.Construction;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_BUILDPROJECT_RESOURCE_MISSING"), "Not able to finish project " + _project + ", due to missing " + _delta + " " + _resource);

        //public override string SummaryText
        //{
        //    get
        //    {
        //        var sitrepText = "Not able to finish project " + _project + ", due to missing " + _delta + " " + _resource;
        //        //var sitrepText = "Not able to finish project {0}, due to missing {1} {2}";
        //        return sitrepText;
        //        //return string.Format(ResourceManager.GetString("SITREP_BUILDPROJECT_RESOURCE_MISSING"),
        //        //    ResourceManager.GetString(ItemType.Name),
        //        //    GameContext.Current.Universe.Map[Location].Name,
        //        //    _isActive ? "" : " (unpowered)");
        //    }
        //}
    }

    [Serializable]
    public class BuildQueueEmptySitRepEntry : SitRepEntry
    {
        private readonly int _col_ID;
        private readonly bool _shipyardQueue;

        public BuildQueueEmptySitRepEntry(Civilization owner, Colony colony, bool shipyardQueue)
            : base(owner, SitRepPriority.Yellow)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _col_ID = colony.ObjectID;
            _shipyardQueue = shipyardQueue;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_col_ID);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus | SitRepCategory.Construction;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _shipyardQueue
                    ? string.Format(
                        ResourceManager.GetString("SITREP_SHIPYARD_BUILD_QUEUE_EMPTY"),
                        Colony.Name, Colony.Location)
                    : string.Format(
                    ResourceManager.GetString("SITREP_PLANETARY_BUILD_QUEUE_EMPTY"),
                    Colony.Name, Colony.Location);
    }

    [Serializable]
    public class CreditsStolenAttackerSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        private readonly int _creditsStolen;
        public CreditsStolenAttackerSitRepEntry(Civilization owner, Colony target, int creditsStolen)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = target.ObjectID;
            _creditsStolen = creditsStolen;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => Colony.Sector;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_CREDITS_STOLEN_SUCCESSFULLY"), _creditsStolen, Colony.Name, Colony.Location);
        //"Our agents stole {0} credits from the treasury on {1}.",
        public override bool IsPriority => true;

    }
    // End of SitRepEntry

    [Serializable]
    public class LaborToEnergyAddedSitRepEntry : SitRepEntry
    {
        private readonly string _note;
        private readonly MapLocation _loc;

        public LaborToEnergyAddedSitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Gray)
        { _loc = loc; _note = Note; }

        public string Note => _note;
        public override SitRepCategory Categories => SitRepCategory.Construction;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _note;
    }
    // End of SitRepEntry

    [Serializable]
    public class CreditsStolenTargetSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        private readonly int _creditsStolen;
        public CreditsStolenTargetSitRepEntry(Civilization owner, Colony target, int creditsStolen)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = target.ObjectID;
            _creditsStolen = creditsStolen;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_CREDITS_WERE_STOLEN"), _creditsStolen, Colony.Name, Colony.Location);
        // {0} credits were stolen from our treasury on { 1}.
        public override bool IsPriority => true;
    }

    [Serializable]
    public class DeCamouflagedSitRepEntry : SitRepEntry
    {
        private readonly string _name;
        private readonly MapLocation _location;
        private readonly string _shipType;
        private readonly int _scanPower;
        public DeCamouflagedSitRepEntry(Orbital orbital, int scanpower)
        : base(orbital.Owner, SitRepPriority.Orange)
        {
            if (orbital == null)
            {
                throw new ArgumentNullException("orbital");
            }

            _name = orbital.Name;
            _shipType = orbital.OrbitalDesign.ShipType;
            _location = orbital.Location;
            _scanPower = scanpower;
        }
        public override SitRepCategory Categories => SitRepCategory.Military;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_location];
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(
                    ResourceManager.GetString("SITREP_SHIP_DECAMOUFLAGED"),
                    ResourceManager.GetString(_name),
                    _shipType,
                    _location,
                    _scanPower);


    }

    [Serializable]
    public class EnergyShutdownSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public EnergyShutdownSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Pink)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);

        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("ENERGY_SHUTDOWN_SHIPYARD_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("ENERGY_SHUTDOWN_SHIPYARD_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/Earthquake.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("ENERGY_SHUTDOWN_SHIPYARD_SUMMARY_TEXT"), Colony.Name, Colony.Location);
        public override bool IsPriority => true;

    }

    [Serializable]
    public class EnergyShutdownBuildingSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        private readonly Colony _colony;
        public EnergyShutdownBuildingSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Pink)
        {
            _colony = colony ?? throw new ArgumentNullException("colony");
            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override string SitRepComment { get; set; }
        public override bool IsPriority => true;
        public override string SummaryText => string.Format(ResourceManager.GetString("ENERGY_SHUTDOWN_BUILDING_SUMMARY_TEXT"), _colony.Name, _colony.Location);

    }

    [Serializable]
    public sealed class DiplomaticSitRepEntry : SitRepEntry
    {
        private readonly IDiplomaticExchange _exchange;

        [NonSerialized]
        private string _summaryText;
        [NonSerialized]
        private string _detailText;
        [NonSerialized]
        private string _image;
        [NonSerialized]
        private bool _hasEvaluatedSummaryText;
        [NonSerialized]
        private bool _hasEvaluatedDetailText;

        public DiplomaticSitRepEntry(Civilization owner, IDiplomaticExchange exchange)
            : base(owner, SitRepPriority.Blue)
        {
            _exchange = exchange ?? throw new ArgumentNullException("exchange");
        }

        private string EnsureText(ref string text, ref bool resolved, bool detailed)
        {
            if (resolved)
            {
                return text;
            }

            DiplomacySitRepStringKey? key = ResolveTextKey(detailed);
            if (key == null)
            {
                resolved = true;
                text = null;
                return text;
            }


            if (!LocalizedTextDatabase.Instance.Groups.TryGetValue(typeof(DiplomacySitRepStringKey), out LocalizedTextGroup textGroup) ||
                !textGroup.Entries.TryGetValue(key.Value, out LocalizedString localizedString))
            {
                resolved = true;
                text = string.Format("!!! MISSING TEXT: {0}.{1} !!!", typeof(DiplomacySitRepStringKey).Name, key);
                return text;
            }

            //GameLog.Client.Diplomacy.DebugFormat("LocalizedText localString ={0}", localizedString.ToString());

            ScriptParameters scriptParameters = new ScriptParameters(
                new ScriptParameter("$sender", typeof(Civilization)),
                new ScriptParameter("$recipient", typeof(Civilization)));

            ScriptExpression scriptExpression = new ScriptExpression(returnObservableResult: false)
            {
                Parameters = scriptParameters,
                ScriptCode = StringHelper.QuoteString(localizedString.LocalText)
            };

            Civilization sender;
            Civilization recipient;

            if (_exchange is IResponse response)
            {
                switch (response.ResponseType)
                {
                    case ResponseType.Accept:
                    case ResponseType.Reject:
                        sender = response.Proposal.Sender;
                        recipient = response.Proposal.Recipient;
                        break;
                    case ResponseType.Counter:
                        sender = response.CounterProposal.Sender;
                        recipient = response.CounterProposal.Recipient;
                        break;
                    default:
                        resolved = true;
                        text = string.Format("!!! ERROR: UNEXPECTED RESPONSE TYPE: {0} !!!", response.ResponseType);
                        return text;
                }
            }
            else
            {
                sender = _exchange.Sender;
                recipient = _exchange.Recipient;
            }

            RuntimeScriptParameters parameters = new RuntimeScriptParameters
                             {
                                 new RuntimeScriptParameter(scriptParameters[0], sender),
                                 new RuntimeScriptParameter(scriptParameters[1], recipient)
                             };

            return scriptExpression.Evaluate<string>(parameters);
        }

        private DiplomacySitRepStringKey? ResolveTextKey(bool detailed)
        {
            IProposal proposal = _exchange as IProposal;
            IResponse response = _exchange as IResponse;

            if (proposal == null && response != null && response.ResponseType == ResponseType.Counter)
            {
                proposal = response.CounterProposal;
            }

            if (proposal != null)
            {
                if (proposal.HasTreaty()) // has clause of treaty type including WarPact
                {
                    if (proposal.HasClause(ClauseType.TreatyCeaseFire))
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.CeaseFireProposedSummaryText;
                    }

                    if (proposal.HasClause(ClauseType.TreatyNonAggression))
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.NonAggressionPactProposedSummaryText;
                    }

                    if (proposal.HasClause(ClauseType.TreatyOpenBorders) /*|| proposal.HasClause(ClauseType.TreatyTradePact)*/)
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.OpenBordersProposedSummaryText;
                    }

                    if (proposal.HasClause(ClauseType.TreatyAffiliation))
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.AffiliationProposedSummaryText;
                    }

                    if (proposal.HasClause(ClauseType.TreatyDefensiveAlliance))
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.DefensiveAllianceProposedSummaryText;
                    }

                    if (proposal.HasClause(ClauseType.TreatyFullAlliance))
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.FullAllianceProposedSummaryText;
                    }

                    if (proposal.HasClause(ClauseType.TreatyMembership))
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.MembershipProposedSummaryText;
                    }
                }

                if (proposal.IsGift())
                {
                    return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.GiftOfferedSummaryText;
                }

                if (proposal.IsDemand())
                {
                    return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.TributeDemandedSummaryText;
                }

                if (proposal.IsWarPact())
                {
                    return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.WarPactProposedSummaryText;
                }
            }

            if (response != null)
            {
                proposal = response.Proposal;

                if (response.ResponseType == ResponseType.Accept)
                {
                    if (proposal.HasTreaty())
                    {
                        if (proposal.HasClause(ClauseType.TreatyCeaseFire))
                        {
                            return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.CeaseFireAcceptedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyNonAggression))
                        {
                            return detailed ? DiplomacySitRepStringKey.NonAggressionPactAcceptedDetailText : DiplomacySitRepStringKey.NonAggressionPactAcceptedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyOpenBorders) /*|| proposal.HasClause(ClauseType.TreatyTradePact)*/)
                        {
                            return detailed ? DiplomacySitRepStringKey.OpenBordersAcceptedDetailText : DiplomacySitRepStringKey.OpenBordersAcceptedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyAffiliation))
                        {
                            return detailed ? DiplomacySitRepStringKey.AffiliationAcceptedDetailText : DiplomacySitRepStringKey.AffiliationAcceptedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyDefensiveAlliance))
                        {
                            return detailed ? DiplomacySitRepStringKey.DefensiveAllianceAcceptedDetailText : DiplomacySitRepStringKey.DefensiveAllianceAcceptedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyFullAlliance))
                        {
                            return detailed ? DiplomacySitRepStringKey.FullAllianceAcceptedDetailText : DiplomacySitRepStringKey.FullAllianceAcceptedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyMembership))
                        {
                            return detailed ? DiplomacySitRepStringKey.MembershipAcceptedDetailText : DiplomacySitRepStringKey.MembershipAcceptedSummaryText;
                        }
                    }

                    if (proposal.IsDemand())
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.TributeAcceptedSummaryText;
                    }

                    if (proposal.IsWarPact())
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.WarPactAcceptedSummaryText;
                    }
                }
                else if (response.ResponseType == ResponseType.Reject)
                {
                    if (proposal.HasTreaty())
                    {
                        if (proposal.HasClause(ClauseType.TreatyCeaseFire))
                        {
                            return DiplomacySitRepStringKey.CeaseFireRejectedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyNonAggression))
                        {
                            return DiplomacySitRepStringKey.NonAggressionPactRejectedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyOpenBorders) /*|| proposal.HasClause(ClauseType.TreatyTradePact)*/)
                        {
                            return DiplomacySitRepStringKey.OpenBordersRejectedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyAffiliation))
                        {
                            return DiplomacySitRepStringKey.AffiliationRejectedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyDefensiveAlliance))
                        {
                            return DiplomacySitRepStringKey.DefensiveAllianceRejectedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyFullAlliance))
                        {
                            return DiplomacySitRepStringKey.FullAllianceRejectedSummaryText;
                        }

                        if (proposal.HasClause(ClauseType.TreatyMembership))
                        {
                            return DiplomacySitRepStringKey.MembershipRejectedSummaryText;
                        }
                    }

                    if (proposal.IsDemand())
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.TributeRejectedSummaryText;
                    }

                    if (proposal.IsWarPact())
                    {
                        return detailed ? default(DiplomacySitRepStringKey?) : DiplomacySitRepStringKey.WarPactRejectedSummaryText;
                    }
                }
            }

            if (_exchange is Statement statement)
            {
                if (statement.StatementType == StatementType.WarDeclaration)
                {
                    return detailed ? DiplomacySitRepStringKey.WarDeclaredDetailText : DiplomacySitRepStringKey.WarDeclaredSummaryText;
                }
            }

            return null;
        }
        public override SitRepCategory Categories => SitRepCategory.Diplomacy;
        public override string SitRepComment { get; set; }
        public override string SummaryText => EnsureText(ref _summaryText, ref _hasEvaluatedSummaryText, false);

        //public override string SummaryText => string.Format(ResourceManager.GetString("EARTHQUAKE_SUMMARY_TEXT"), Colony.Name, Colony.Location); 
        public override string DetailText => EnsureText(ref _detailText, ref _hasEvaluatedDetailText, true);

        public override string DetailImage
        {
            get
            {
                if (_hasEvaluatedDetailText)
                {
                    return _image;
                }

                if (!HasDetails)
                {
                    _image = null;
                    return _image;
                }

                Civilization sender;
                Civilization recipient;

                if (_exchange is IResponse response)
                {
                    switch (response.ResponseType)
                    {
                        case ResponseType.Accept:
                        case ResponseType.Reject:
                            sender = response.Proposal.Sender;
                            recipient = response.Proposal.Recipient;
                            break;
                        case ResponseType.Counter:
                            sender = response.CounterProposal.Sender;
                            recipient = response.CounterProposal.Recipient;
                            break;
                        default:
                            _image = null;
                            return _image;
                    }
                }
                else
                {
                    sender = _exchange.Sender;
                    recipient = _exchange.Recipient;
                }

                _image = Owner == sender ? recipient.InsigniaPath : sender.InsigniaPath;

                return _image;
            }
        }
    }

    [Serializable]
    public class EarthquakeSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public EarthquakeSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Pink)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("EARTHQUAKE_SUMMARY_TEXT"), Colony.Name, Colony.Location);
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("EARTHQUAKE_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("EARTHQUAKE_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/Earthquake.png";
    }

    [Serializable]
    public class FirstContactSitRepEntry : SitRepEntry
    {
        private readonly int _civilizationID;
        private readonly MapLocation _location;
        public FirstContactSitRepEntry(Civilization owner, Civilization civilization, MapLocation location)
            : base(owner, SitRepPriority.Blue)
        {
            if (civilization == null)
            {
                throw new ArgumentNullException("civilization");
            }

            _civilizationID = civilization.CivID;
            _location = location;
        }
        public override SitRepAction Action => SitRepAction.ShowDiploScreen;
        public Civilization Civilization => GameContext.Current.Civilizations[_civilizationID];
        public MapLocation Location => _location;
        public Sector Sector => GameContext.Current.Universe.Map[Location];
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string DetailImage => Civilization.Image;
        public override string DetailText => Civilization.DiplomacyReport ?? Civilization.Race.Description;
        public override SitRepCategory Categories => SitRepCategory.Diplomacy | SitRepCategory.FirstContact;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_FIRST_CONTACT"), ResourceManager.GetString(Civilization.ShortName), Sector);
        public override bool IsPriority => true;
    }

    [Serializable]
    public class FoodReservesDestroyedAttackerSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly int _destroyedFoodReserves;
        public FoodReservesDestroyedAttackerSitRepEntry(Civilization owner, Colony target, int destroyedFoodReserves)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
            _destroyedFoodReserves = destroyedFoodReserves;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;

        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_FOOD_RESERVES_DESTROYED_SUCCESSFULLY"), System.Name, System.Location, _destroyedFoodReserves);
        public override string SitRepComment { get; set; }
        public override bool IsPriority => true;
    }

    [Serializable]
    public class GammaRayBurstSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public GammaRayBurstSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Pink)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override bool IsPriority => true;
        public override object ActionTarget => Colony;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("GAMMA_RAY_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("GAMMA_RAY_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/GammaRayBurst.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("GAMMA_RAY_BURST_SUMMARY_TEXT"), Colony.Name, Colony.Location);
    }

    [Serializable]
    public class GrowthByHealthSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        //private readonly int _shipID;
        //private readonly string _researchNote;

        public GrowthByHealthSitRepEntry(Civilization owner, Colony colony)
                : base(owner, SitRepPriority.RedYellow)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }

        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        //public string ResearchNote => _researchNote; 
        public override SitRepCategory Categories => SitRepCategory.Research;  // not used atm
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override string SitRepComment { get; set; }
        public override string SummaryText
        {
            get
            {
                string _text = string.Format(ResourceManager.GetString("SITREP_GROWTH_BY_HEALTH_UNKNOWN_COLONY_TEXT"));
                if (Colony != null)
                {
                    _text = string.Format(ResourceManager.GetString("SITREP_GROWTH_BY_HEALTH_TEXT"), Colony.Name, Colony.Location);
                }

                return _text;
            }
        }
        public override bool IsPriority => true;
    }
    // End of GrowthByHealthSitRepEntry

    [Serializable]
    public class IntelAttackFailedSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;

        public IntelAttackFailedSitRepEntry(Civilization owner, Colony target)
            : base(owner, SitRepPriority.Orange)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
        }

        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override bool IsPriority => true;
        //public string SitRepComment => "no"; } set { _sitRepComment = value; 
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_AGENTS_FAILED"), System.Name);
        //"Our agents have failed in their mission on {0}",

    }

    [Serializable]
    public class IntelDefenceSucceededSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;

        public IntelDefenceSucceededSitRepEntry(Civilization owner, Colony target)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override bool IsPriority => true;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => System.Sector;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_AGENTS_FOILED_PLOT"), System.Name);
        //"Our agents have foiled a plot by spies on {0}",
    }

    [Serializable]
    public class FoodReservesDestroyedTargetSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly int _destroyedFoodReserves;
        public FoodReservesDestroyedTargetSitRepEntry(Civilization owner, Colony target, int destroyedFoodReserves)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
            _destroyedFoodReserves = destroyedFoodReserves;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_FOOD_RESERVES_DESTROYED"), System.Name, System.Location, _destroyedFoodReserves);

    }

    [Serializable]
    public class ItemBuiltSitRepEntry : SitRepEntry
    {
        private readonly int _itemTypeId;
        private readonly MapLocation _location;
        public ItemBuiltSitRepEntry(Civilization owner, TechObjectDesign itemType, MapLocation location, SitRepPriority priority)
            : base(owner, SitRepPriority.Green)
        {
            if (itemType == null)
            {
                throw new ArgumentNullException("itemType");
            }

            _itemTypeId = itemType.DesignID;
            _location = location;
            //GameLog.Client.SitReps.DebugFormat("SR: "+ SummaryText);
        }
        public TechObjectDesign ItemType => GameContext.Current.TechDatabase[_itemTypeId];
        public MapLocation Location => _location;
        public override SitRepCategory Categories => SitRepCategory.Construction;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_CONSTRUCTED_I")
            , GameContext.Current.Universe.Map[Location].Name
            , Location
            , ResourceManager.GetString(ItemType.Name));
    }

    [Serializable]
    public class MajorAsteroidImpactSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public MajorAsteroidImpactSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Orange)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colonyName missing for MajorAsteroidImpact");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override bool IsPriority => true;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("MAJOR_ASTEROID_STRIKE_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("MAJOR_ASTEROID_STRIKE_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/MajorAsteroidImpact.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("MAJOR_ASTEROID_STRIKE_SUMMARY_TEXT"), Colony.Name, Colony.Location);
    }

    [Serializable]
    public class NegativeTreasurySitRepEntry : SitRepEntry
    {
        public NegativeTreasurySitRepEntry(Civilization owner)
            : base(owner, SitRepPriority.Red) { }
        public override SitRepCategory Categories => SitRepCategory.General;

        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_NEGATIVE_TREASURY"));
        //return "Your empire is out of funds and cannot pay its ship's maintenance.\nShips cannot repair hull damage and are degrading.";
    }

    [Serializable]
    public class NewColonySitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public NewColonySitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Blue)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.NewColony;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_NEW_COLONY_ESTABLISHED"), Colony.Sector.Name, Colony.Location);

        public override bool IsPriority => true;

    }

    [Serializable]
    public class DenounceWarSitRepEntry : SitRepEntry
    {
        private readonly int _victimCivilizationID;
        private readonly int _ownerCivilizationID;
        private readonly int _denouncerCivilizationID;
        private readonly CivString _detailText;

        public override SitRepCategory Categories => SitRepCategory.Diplomacy | SitRepCategory.Military;


        public override SitRepAction Action => SitRepAction.CenterOnSector;


        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_DENOUNCE_WAR"),
                    Denouncer.LongName, Owner.LongName, Victim.LongName);

        public override string DetailImage => Denouncer.InsigniaPath;

        public override string DetailText => string.Format(_detailText.Value, Owner.LongName, Victim.LongName);

        public override bool IsPriority => true;


        public Civilization Victim => GameContext.Current.Civilizations[_victimCivilizationID];

        public Civilization OwnerCiv => GameContext.Current.Civilizations[_ownerCivilizationID];

        public Civilization Denouncer => GameContext.Current.Civilizations[_denouncerCivilizationID];
        public override string SitRepComment { get; set; }

        public DenounceWarSitRepEntry(Civilization denouncer, Civilization ownerCiv, Civilization victim)
            : base(ownerCiv, SitRepPriority.Red)
        {
            if (ownerCiv == null)
            {
                throw new ArgumentNullException("owmer");
            }

            if (victim == null)
            {
                throw new ArgumentNullException("victim");
            }

            if (denouncer == null)
            {
                throw new ArgumentNullException("denouncer");
            }

            _denouncerCivilizationID = denouncer.CivID;
            _ownerCivilizationID = ownerCiv.CivID;
            _victimCivilizationID = victim.CivID;
            _detailText = new CivString(
                    ownerCiv,
                    victim,
                    CivString.DiplomacyCategory,
                    "MESSAGE_SITREP_DETAILS_DENOUNCE_WAR_THEM");
        }
    }

    [Serializable]
    public class CombatSummarySitRepEntry : SitRepEntry
    {
        private readonly string _note;
        private readonly MapLocation _loc;

        public CombatSummarySitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Purple)
        { _loc = loc; _note = Note; }

        public string Note => _note;
        public override SitRepCategory Categories => SitRepCategory.Military;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _note;

    }
    // End of SitRepEntry


    [Serializable]
    public class CommendWarSitRepEntry : SitRepEntry
    {
        private readonly int _victimCivilizationID;
        private readonly int _ownerCivilizationID;
        private readonly int _commenderCivilizationID;
        private readonly CivString _detailText;

        public override SitRepCategory Categories => SitRepCategory.Diplomacy | SitRepCategory.Military;

        public override SitRepAction Action => SitRepAction.CenterOnSector;

        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_COMMEND_WAR"),
                    Commender.LongName, Owner.LongName, Victim.LongName);

        public override string DetailImage => Commender.InsigniaPath;

        public override string DetailText => string.Format(_detailText.Value, Owner.LongName, Victim.LongName);
        public override string SitRepComment { get; set; }
        public override bool IsPriority => true;

        public Civilization Victim => GameContext.Current.Civilizations[_victimCivilizationID];

        public Civilization OwnerCiv => GameContext.Current.Civilizations[_ownerCivilizationID];

        public Civilization Commender => GameContext.Current.Civilizations[_commenderCivilizationID];

        public CommendWarSitRepEntry(Civilization commender, Civilization ownerCiv, Civilization victim)
            : base(ownerCiv, SitRepPriority.Red)
        {
            if (ownerCiv == null)
            {
                throw new ArgumentNullException("owmer");
            }

            if (victim == null)
            {
                throw new ArgumentNullException("victim");
            }

            if (commender == null)
            {
                throw new ArgumentNullException("commender");
            }

            _commenderCivilizationID = commender.CivID;
            _ownerCivilizationID = ownerCiv.CivID;
            _victimCivilizationID = victim.CivID;
            _detailText = new CivString(
                    ownerCiv,
                    victim,
                    CivString.DiplomacyCategory,
                    "MESSAGE_SITREP_DETAILS_COMMEND_WAR_THEM");
        }
    }

    [Serializable]
    public class NewInfiltrateSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly int _gainedResearchPointsSum;
        private readonly int _gainedOfTotalResearchPoints;
        public NewInfiltrateSitRepEntry(Civilization owner, Colony colony, int gainedResearchPointsSum, int gainedOfTotalResearchPoints)
            : base(owner, SitRepPriority.Red)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = colony.System.ObjectID;

            _gainedResearchPointsSum = gainedResearchPointsSum;
            _gainedOfTotalResearchPoints = gainedOfTotalResearchPoints;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText
        {
            get
            {
                if (_gainedResearchPointsSum > 0)
                {
                    return string.Format(ResourceManager.GetString("SITREP_INFILTRATE_SUCCESSFULLY"),
                        //"Our spies have infiltrated the {0} at {1} and gained {2} of {3} research points.",
                        System.Owner, System.Name, _gainedResearchPointsSum, _gainedOfTotalResearchPoints);
                }
                else
                {
                    return string.Format(ResourceManager.GetString("SITREP_INFILTRATE_NO_SUCCESS"),
                        //"Our spies have tried to infiltrate the {0} at {1} but they had no success.",
                        System.Owner, System.Name);
                }
            }
        }
    }



    [Serializable]
    public class NewInfluenceSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly int _gainedCreditsSum;
        private readonly int _gainedOfTotalCredits;
        public NewInfluenceSitRepEntry(Civilization owner, Colony colony, int gainedCreditsSum, int gainedOfTotalCredits)
            : base(owner, SitRepPriority.Red)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = colony.System.ObjectID;

            _gainedCreditsSum = gainedCreditsSum;
            _gainedOfTotalCredits = gainedOfTotalCredits;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText
        {
            get
            {
                if (_gainedCreditsSum > 0)
                {
                    return string.Format(ResourceManager.GetString("SITREP_INFLUENCE_NO_SUCCESS"),
                        //"The {0} at {1} have been influenced: we got {2} of {3} credits.",
                        System.Owner, System.Name, _gainedCreditsSum, _gainedOfTotalCredits);
                }
                else
                {
                    return string.Format(ResourceManager.GetString("SITREP_INFLUENCE_NO_SUCCESS"),
                        //"Our spies have tried to influence the {0} at {1} but they had no success.",
                        System.Owner, System.Name);
                }
            }
        }


    }

    //[Serializable]
    //public class NewRaidSitRepEntry : SitRepEntry
    //{
    //    private readonly int _systemId;
    //    private readonly int _gainedCredits;
    //    private readonly int _totalCredits;

    //public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId); 
    //public override SitRepAction Action => SitRepAction.CenterOnSector;
    //public override object ActionTarget => System.Sector; 
    //public override SitRepCategory Categories => SitRepCategory.ColonyStatus;

    //    public override string SummaryText
    //    {
    //        get
    //        {
    //            if (_gainedCredits > 0)
    //            {
    //                return string.Format(ResourceManager.GetString("SITREP_RAID_SUCCESSFULLY"),
    //                    //"The {0} at {1} have been raided: we got {2} of {3} credits.",
    //                    System.Owner, System.Name, _gainedCredits, _totalCredits);
    //            }
    //            else
    //            {
    //                return string.Format(ResourceManager.GetString("SITREP_RAID_NO_SUCCESS"),
    //                    //"Our spies have tried to raid the {0} at {1} but they had no success.",
    //                    System.Owner, System.Name);
    //            }
    //        }
    //    }
    //public override bool IsPriority => true;
    //    public NewRaidSitRepEntry(Civilization owner, Colony colony, int gainedCredits, int totalCredits)
    //        : base(owner, SitRepPriority.Red)
    //    {
    //        if (colony == null)
    //            throw new ArgumentNullException("colony");
    //        _systemId = colony.System.ObjectID;

    //        _gainedCredits = gainedCredits;
    //        _totalCredits = totalCredits;
    //    }
    //}

    [Serializable]
    public class NewSabotagedSitRepEntry : SitRepEntry
    {
        private readonly Civilization _attacked;
        private readonly Civilization _attacking;
        private readonly int _systemId;
        private readonly int _removedStuff;
        private readonly int _totalStuff;
        private readonly string _affectedField;
        private readonly string _blamed;
        private readonly int _ratioLevel;
        //private readonly string _roleText;

        public NewSabotagedSitRepEntry(Civilization attacking, Civilization attacked, Colony colony
            , string affectedField, int removedStuff, int totalStuff, string blamed, int ratioLevel)
            : base(attacking, SitRepPriority.Red) // owner is the attackED for this, the sabotaged sit rep
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _attacked = attacked;
            _attacking = attacking;
            _systemId = colony.System.ObjectID;
            _removedStuff = removedStuff;  // facilities or credits or research points 
            _totalStuff = totalStuff;
            _affectedField = affectedField;
            _blamed = blamed;
            _ratioLevel = ratioLevel;
            //string _blamedString = BlamedString;
        }
        public string Attacking => _attacking.Key;
        public string Attacked => _attacked.Key;
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override bool IsPriority => true;
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string DetailImage => "vfs:///Resources/Images/Intelligence/IntelMission.png";
        public override string HeaderText => string.Format(ResourceManager.GetString("NEW_SABOTAGED_HEADER_TEXT"));
        //public override string HeaderText => string.Format(ResourceManager.GetString("NEW_SABOTAGED_HEADER_TEXT"), Colony.Name, Colony.Location); 
        public override string DetailText
        {
            get
            {
                string _detailText = SummaryText;
                _detailText = _detailText.Replace("  ", "[nl][nl]");

                return _detailText;
            }
        }
        //public override string DetailText => string.Format(ResourceManager.GetString("NEW_SABOTAGED_DETAIL_TEXT"), Colony.Name, Colony.Location); 
        public string RoleString => string.Format(ResourceManager.GetString("SABOTAGE_ROLE_ATTACKED_CIV"));
        public string BlamedString => _blamed + " " + RatioLevelString;
        public string RatioLevelString
        {
            get
            {
                string ratioLevelString = "";
                switch (_ratioLevel)
                {
                    case 1: ratioLevelString = ResourceManager.GetString("SITREP_SABOTAGE_CONFIDENCE_LEVEL_1"); break;
                    case 2: ratioLevelString = ResourceManager.GetString("SITREP_SABOTAGE_CONFIDENCE_LEVEL_2"); break;
                    case 3: ratioLevelString = ResourceManager.GetString("SITREP_SABOTAGE_CONFIDENCE_LEVEL_3"); break;
                    default:
                        break;
                }
                return ratioLevelString;
            }
        }
        public override string SitRepComment { get; set; }
        public override string SummaryText
        {
            get
            {
                {

                    if (_removedStuff == -2)
                    {

                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_NOT_WORTH"),
                            RoleString, System.Name, System.Location, System.Owner, _affectedField, _blamed);
                        ////    0               1          2                 3              4        5   placeholders in en.txt
                        ///
                        //return "We were attacked by " + _attacking.ShortName  + " we did not have enough " + _affectedField + " to bother with.";
                    }
                    if (_removedStuff == -1)
                    {

                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_FAILED"),
                            RoleString, System.Name, System.Location, System.Owner, _affectedField);
                        ////    0               1          2                 3              4       placeholders in en.txt
                        ///
                        //return "We were attacked by " + _attacking.ShortName + " we did not have enough " + _affectedField + " to bother with";
                    }

                    if (_removedStuff > 0)
                    {

                        string destroyed = string.Format(ResourceManager.GetString("SITREP_SABOTAGE_DESTROYED"));
                        //return "Holy crap!, We were attacked by " + _attacking.ShortName + ". They got " + _affectedField + "!";

                        if (_affectedField == ResourceManager.GetString("SITREP_SABOTAGE_CREDITS_SABOTAGED") ||
                            _affectedField == ResourceManager.GetString("SITREP_SABOTAGE_RESEARCH_SABOTAGED"))
                        {
                            destroyed = string.Format(ResourceManager.GetString("SITREP_SABOTAGE_STOLEN"));
                        }

                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED"),  // {0} {2} facility/facilities sabotaged on {1}.
                           RoleString, System.Name, System.Location, _affectedField, _removedStuff, _totalStuff + _removedStuff,
                            //// 0               1          2             3                   4               5   
                            BlamedString, System.Owner, destroyed);
                        ////            6                   7                 8                
                    }
                    else // _removedStuff = 0
                    {

                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_FAILED"),
                            RoleString, System.Name, System.Location, System.Owner, _affectedField);
                        ////    0               1          2                 3              4   placeholders in en.txt
                        ///
                        //return "Fake News, We were attacked by " + _attacking.ShortName + " but the mission on " + _affectedField + " failed!";
                    }
                }
            }
        }
    }

    [Serializable]
    public class NewSabotagingSitRepEntry : SitRepEntry // local is being sabotaged
    {
        private readonly Civilization _attacked;
        //private readonly Civilization _attacking;
        private readonly int _systemId;
        private readonly int _removedStuff;
        private readonly int _totalStuff;
        private readonly string _affectedField;
        private readonly string _blamed;
        private readonly int _ratioLevel;


        public NewSabotagingSitRepEntry(Civilization owner, Civilization attacked, Colony colony, string affectedField, int removedStuff, int totalStuff, string blame, int ratioLevel)
            : base(owner, SitRepPriority.Red) // owner is the attackING for this, the sabotagING sit rep
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }
            //_attacked = owner;
            _attacked = attacked;
            _systemId = colony.System.ObjectID;
            _removedStuff = removedStuff;  // facilities or credits or research points 
            _totalStuff = totalStuff;
            _affectedField = affectedField;
            _blamed = blame;
            _ratioLevel = ratioLevel;
        }

        public string Attacked => _attacked.Key;
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("NEW_SABOTAGING_HEADER_TEXT"));
        //public override string DetailText => string.Format(ResourceManager.GetString("NEW_SABOTAGING_DETAIL_TEXT"); 
        public override string DetailImage => "vfs:///Resources/Images/Intelligence/IntelMission.png";
        public override string DetailText
        {
            get
            {
                string _detailText = SummaryText;
                _detailText = _detailText.Replace("  ", "[nl][nl]");

                return _detailText;
            }
        }
        public override bool IsPriority => true;
        public string RoleString => string.Format(ResourceManager.GetString("SABOTAGE_ROLE_ATTACKING_CIV"));

        public string BlamedString => _blamed + " " + RatioLevelString;

        public string RatioLevelString
        {
            get
            {
                string ratioLevelString = "";
                switch (_ratioLevel)
                {
                    case 1: ratioLevelString = ResourceManager.GetString("SITREP_SABOTAGE_CONFIDENCE_LEVEL_1"); break;
                    case 2: ratioLevelString = ResourceManager.GetString("SITREP_SABOTAGE_CONFIDENCE_LEVEL_2"); break;
                    case 3: ratioLevelString = ResourceManager.GetString("SITREP_SABOTAGE_CONFIDENCE_LEVEL_3"); break;
                    default:
                        break;
                }
                return ratioLevelString;
            }
        }
        public override string SitRepComment { get; set; }
        public override string SummaryText
        {
            get
            {
                {
                    if (_removedStuff == -2)
                    {
                        //return "We attacked " + _attacked.ShortName + " but they did not have enough " + _affectedField;
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_NOT_WORTH"),
                            RoleString, System.Name, System.Location, System.Owner, _affectedField, _blamed);
                        //    0               1          2                 3              4   5   placeholders in en.txt
                    }
                    if (_removedStuff == -1)
                    {
                        //return "We attacked " + _attacked.ShortName + " but the mission after " + _affectedField + " failed!";
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_FAILED"),
                            RoleString, System.Name, System.Location, System.Owner, _affectedField);
                        //    0               1          2                 3              4       placeholders in en.txt
                    }
                    if (_removedStuff > 0)
                    {
                        //return "We attacked " + _attacked.ShortName + " for " + _affectedField;  // e.g. for credits or for food facilites


                        string destroyed = ResourceManager.GetString("SITREP_SABOTAGE_DESTROYED");
                        if (_affectedField == ResourceManager.GetString("SITREP_SABOTAGE_CREDITS_SABOTAGED") ||
                            _affectedField == ResourceManager.GetString("SITREP_SABOTAGE_RESEARCH_SABOTAGED"))
                        {
                            destroyed = ResourceManager.GetString("SITREP_SABOTAGE_STOLEN");
                        }

                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED"),  // {0} {2} facility/facilities sabotaged on {1}.
                           RoleString, System.Name, System.Location, _affectedField, _removedStuff, _totalStuff + _removedStuff, BlamedString, System.Owner, destroyed);
                        ////    0               1          2                 3                   4               5                    6        7           8
                    }
                    else // _removedStuff = 0
                    {
                        //return "Fake news, we attacked " + System.Owner.ShortName + " but the mission on " + _affectedField + " failed!";
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_FAILED"),
                            RoleString, System.Name, System.Location, System.Owner, _affectedField);
                        //    0               1          2                 3              4   placeholders in en.txt
                    }
                }
            }
        }

    }

    //[Serializable]
    //public class NewSabotageFromShipSitRepEntry : SitRepEntry // local is sabotaging someone
    //{
    //    private readonly int _systemId;
    //    private readonly int _removeEnergyFacilities;
    //    private readonly int _totalEnergyFacilities;

    //public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId); 
    //public override SitRepAction Action => SitRepAction.CenterOnSector;
    //public override object ActionTarget => System.Sector; 
    //public override SitRepCategory Categories => SitRepCategory.ColonyStatus; 

    //    public override string SummaryText
    //    {
    //        get
    //        {
    //            if (_removeEnergyFacilities > 0)
    //            {
    //                return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_SUCCESS"),
    //                //"Successful sabotage mission to {0} {1}, (ship lost in action): {2} of {3} energy facilities destroyed.",
    //                   System.Owner, System.Location, _removeEnergyFacilities, _totalEnergyFacilities + _removeEnergyFacilities);
    //            }
    //            else
    //            {
    //                return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FAILED"),
    //                    //"The sabotage mission to {0} at {1} failed and the sabotage ship was lost.",
    //                    System.Owner, System.Name);
    //            }
    //        }
    //    }
    //public override bool IsPriority => true;
    //    public NewSabotageFromShipSitRepEntry(Civilization owner, Colony colony, int removeEnergyFacilities, int totalEnergyFacilities)
    //        : base(owner, SitRepPriority.Red)
    //    {
    //        if (colony == null)
    //            throw new ArgumentNullException("colony");
    //        _systemId = colony.System.ObjectID;

    //        _removeEnergyFacilities = removeEnergyFacilities;
    //        _totalEnergyFacilities = totalEnergyFacilities;
    //    }
    //}

    [Serializable]
    public class OrbitalDestroyedSitRepEntry : SitRepEntry
    {
        private readonly string _name;
        private readonly MapLocation _location;
        private readonly string _shipType;

        public OrbitalDestroyedSitRepEntry(Orbital orbital)
            : base(orbital.Owner, SitRepPriority.Purple)
        {
            if (orbital == null)
            {
                throw new ArgumentNullException("orbital");
            }

            _name = orbital.Name;
            _shipType = orbital.OrbitalDesign.ShipType;
            _location = orbital.Location;
        }
        public string Name => _name;

        public MapLocation Location => _location;

        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[Location];
        public override SitRepCategory Categories => SitRepCategory.Military;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(
                    ResourceManager.GetString("SITREP_ORBITAL_DESTROYED"),
                    ResourceManager.GetString(Name),
                    _shipType,
                    Location);
    }

    [Serializable]
    public class PlanetaryDefenceAttackAttackerSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly int _orbitalBatteriesDestroyed;
        private readonly int _shieldHealthRemoved;
        public PlanetaryDefenceAttackAttackerSitRepEntry(Civilization owner, Colony target, int orbitalBatteriesDestroyed, int shieldHealthRemoved)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
            _orbitalBatteriesDestroyed = orbitalBatteriesDestroyed;
            _shieldHealthRemoved = shieldHealthRemoved;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_SABOTEURS_ATTACKED_PLANETARY_DEFENCES_SUCCESSFULLY"),
                    System.Name, _orbitalBatteriesDestroyed, _shieldHealthRemoved);//Our agents have attacked the planetary defences at { 0}, destroying { 1} orbital batteries and damaged the planetary shields by { 2}.
    }

    [Serializable]
    public class PlanetaryDefenceAttackTargetSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly int _orbitalBatteriesDestroyed;
        private readonly int _shieldHealthRemoved;
        public PlanetaryDefenceAttackTargetSitRepEntry(Civilization owner, Colony target, int orbitalBatteriesDestroyed, int shieldHealthRemoved)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
            _orbitalBatteriesDestroyed = orbitalBatteriesDestroyed;
            _shieldHealthRemoved = shieldHealthRemoved;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_SABOTEURS_ATTACKED_PLANETARY_DEFENCES"),
                    System.Name, _orbitalBatteriesDestroyed, _shieldHealthRemoved);//Saboteurs have attacked the planetary defences at { 0}, destroying { 1} orbital batteries and damaged the planetary shields by { 2}.
    }

    [Serializable]
    public class PlagueSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public PlagueSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Pink)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("PLAGUE_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("PLAGUE_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/Plague.png";
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("PLAGUE_SUMMARY_TEXT"), Colony.Name, Colony.Location);
    }

    //TODO: This needs fleshing out a bit more - needs a definite pop up,
    // image of graveyard or something
    [Serializable]
    public class PopulationDiedSitRepEntry : SitRepEntry
    {
        private readonly MapLocation _loc;
        private readonly string _note;
        public PopulationDiedSitRepEntry(Civilization owner, MapLocation loc, string note) : base(owner, SitRepPriority.Red)
        {
            _loc = loc; _note = note;
        }

        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _note;
        public override bool IsPriority => true;

    }

    //TODO: This needs fleshing out. Need a definite popup,
    //image with something to do with medical or death
    [Serializable]
    public class PopulationDyingSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;

        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);

        public PopulationDyingSitRepEntry(Civilization owner, Colony colony) : base(owner, SitRepPriority.Red)
        {
            if (owner == null)
            {
                throw new ArgumentException("owner");
            }

            if (colony == null)
            {
                throw new ArgumentException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_POPULATION_DYING"), Colony.Name, Colony.Location);
    }

    [Serializable]
    public class ProductionFacilitiesDestroyedAttackerSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly ProductionCategory _facilityType;
        private readonly int _destroyedFacilities;
        public ProductionFacilitiesDestroyedAttackerSitRepEntry(Civilization owner, Colony target, ProductionCategory productionType, int destroyedFacilities)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
            _facilityType = productionType;
            _destroyedFacilities = destroyedFacilities;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText
        {
            get
            {
                switch (_facilityType)
                {
                    case ProductionCategory.Energy:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_ENERGY"),
                            //"We have sabotaged {0} energy facilities on {1}",
                            _destroyedFacilities, System.Name);
                    case ProductionCategory.Food:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_FOOD"),
                            //"We have sabotaged {0} food facilities  on {1}",
                            _destroyedFacilities, System.Name);
                    case ProductionCategory.Industry:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_INDUSTRY"),
                            //"We have sabotaged {0} industrial facilities on {1}",
                            _destroyedFacilities, System.Name);
                    case ProductionCategory.Intelligence:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_INTELLIGENCE"),
                            //"We have sabotaged {0} intelligence facilities on {1}",
                            _destroyedFacilities, System.Name);
                    case ProductionCategory.Research:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_RESEARCH"),
                            //"We have sabotaged {0} research facilities on {1}",
                            _destroyedFacilities, System.Name);
                    default:
                        return null;
                }

            }
        }
    }

    [Serializable]
    public class ProductionFacilitiesDestroyedTargetSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly ProductionCategory _facilityType;
        private readonly int _destroyedFacilities;
        public ProductionFacilitiesDestroyedTargetSitRepEntry(Civilization owner, Colony target, ProductionCategory productionType, int destroyedFacilities)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
            _facilityType = productionType;
            _destroyedFacilities = destroyedFacilities;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText
        {
            get
            {
                switch (_facilityType)
                {
                    case ProductionCategory.Energy:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_ENERGY"),
                            //"{0} energy facilities have been sabotaged on {1}",
                            _destroyedFacilities, System.Name);
                    case ProductionCategory.Food:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_FOOD"),
                            //"{0} food facilities have been sabotaged on {1}",
                            _destroyedFacilities, System.Name);
                    case ProductionCategory.Industry:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_INDUSTRY"),
                            //"{0} industrial facilities have been sabotaged on {1}",
                            _destroyedFacilities, System.Name);
                    case ProductionCategory.Intelligence:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_INTELLIGENCE"),
                            //"{0} intelligence facilities have been sabotaged on {1}",
                            _destroyedFacilities, System.Name);
                    case ProductionCategory.Research:
                        return string.Format(ResourceManager.GetString("SITREP_SABOTAGE_FACILITIES_SABOTAGED_RESEARCH"),
                            //"{0} research facilities have been sabotaged on {1}",
                            _destroyedFacilities, System.Name);
                    default:
                        return null;
                }
            }
        }
    }

    [Serializable]
    public class ReligiousHolidaySitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public ReligiousHolidaySitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Pink)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("RELIGIOUS_HOLIDAY_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("RELIGIOUS_HOLIDAY_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/ReligiousHoliday.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("RELIGIOUS_HOLIDAY_SUMMARY_TEXT"), Colony.Name, Colony.Location);
        public override bool IsPriority => true;

    }

    //[Serializable]
    //public class ReportOutput_Aqua_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Aqua_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Brown)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_Blue_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Blue_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Blue)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_BlueDark_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_BlueDark_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.BlueDark)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry


    //[Serializable]
    //public class ReportOutput_Brown_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Brown_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Brown)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_Gray_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Gray_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Gray)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_Green_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Green_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Green)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_Orange_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Orange_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Orange)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_Pink_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Pink_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Pink)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_Purple_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Purple_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Purple)
    //    { _loc = loc; _note = Note; }

    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_Red_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Red_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Red)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_Yellow_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_Yellow_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Yellow)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    //[Serializable]
    //public class ReportOutput_RedYellow_CoS_SitRepEntry : SitRepEntry
    //{
    //    // CoS = CenterOnSector
    //    private readonly string _note;
    //    private readonly MapLocation _loc;

    //    public ReportOutput_RedYellow_CoS_SitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.RedYellow)
    //    { _loc = loc; _note = Note; }
    //    public string Note => _note;
    //    public override SitRepCategory Categories => SitRepCategory.Construction;
    //    public override SitRepAction Action => SitRepAction.CenterOnSector;
    //    public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
    //    public override bool IsPriority => true;
    //    public override string SitRepComment { get; set; }
    //    public override string SummaryText => _note;
    //}
    //// End of SitRepEntry

    [Serializable]
    public class ResearchCompleteSitRepEntry : SitRepEntry
    {
        private readonly int _applicationId;
        private readonly int[] _newDesignIds;

        public ResearchCompleteSitRepEntry(
            Civilization owner,
            ResearchApplication application,
            ICollection<TechObjectDesign> newDesigns) : base(owner, SitRepPriority.Green)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            _applicationId = application.ApplicationID;
            if (newDesigns != null)
            {
                int i = 0;
                _newDesignIds = new int[newDesigns.Count];
                foreach (TechObjectDesign design in newDesigns)
                {
                    _newDesignIds[i++] = design.DesignID;
                }
            }
        }

        public ResearchApplication Application => GameContext.Current.ResearchMatrix.GetApplication(_applicationId);
        public override SitRepCategory Categories => SitRepCategory.Research;
        public override SitRepAction Action => SitRepAction.ShowScienceScreen;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_RESEARCH_COMPLETED"), ResourceManager.GetString(Application.Name), Application.Level);
        public override bool HasDetails => true; // turn on/off for extra Dialog window

        public override string DetailText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                _ = sb.AppendLine(ResourceManager.GetString(Application.Description));
                if ((_newDesignIds != null) && (_newDesignIds.Length > 0))
                {
                    _ = sb.Append("[nl/]" + ResourceManager.GetString("SITREP_TECHS_NOW_AVAILABLE") + "[nl/]");
                    for (int i = 0; i < _newDesignIds.Length; i++)
                    {
                        TechObjectDesign design = GameContext.Current.TechDatabase[_newDesignIds[i]];
                        if (design == null)
                        {
                            continue;
                        }

                        _ = sb.Append("[nl/]");
                        _ = sb.Append(ResourceManager.GetString(design.Name));

                    }
                }
                return sb.ToString();
            }
        }

        public override string DetailImage
        {
            get
            {
                ResearchField field = Application.Field;
                if (field != null)
                {
                    return field.Image;
                }

                return base.DetailImage;
            }
        }
    }

    [Serializable]
    public class ScienceSummarySitRepEntry : SitRepEntry
    {
        private readonly string _researchNote;

        public ScienceSummarySitRepEntry(Civilization owner, string researchNote)
                : base(owner, SitRepPriority.Blue)
        { _researchNote = researchNote; }

        public string ResearchNote => _researchNote;
        public override SitRepCategory Categories => SitRepCategory.Research;
        public override SitRepAction Action => SitRepAction.ShowScienceScreen;
        public override bool IsPriority => false;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _researchNote;

    }
    // End of SitRepEntry

    [Serializable]
    public class ScienceShipResearchGainedSitRepEntry : SitRepEntry
    {

        private readonly int _shipID;
        private readonly int _researchGained;
        public ScienceShipResearchGainedSitRepEntry(Civilization owner, Ship scienceShip, int researchGained)
            : base(owner, SitRepPriority.Green)
        {
            _shipID = scienceShip.ObjectID;
            _researchGained = researchGained;
        }
        public Ship ScienceShip => GameContext.Current.Universe.Get<Ship>(_shipID);
        //public override string SitRepComment => "no"; } set { 
        public int ResearchGained => _researchGained;
        public override SitRepCategory Categories => SitRepCategory.Research;
        public override SitRepAction Action => SitRepAction.SelectTaskForce;
        public override object ActionTarget => ScienceShip.Fleet;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText
        {
            get
            {
                string StarTypeFullText;

                if (ScienceShip != null) // science ship destroyed < null ref 
                {
                    switch (ScienceShip.Sector.System.StarType)
                    {
                        case StarType.Blue:
                        case StarType.Orange:
                        case StarType.Red:
                        case StarType.White:
                        case StarType.Yellow:
                        case StarType.Wormhole:
                            StarTypeFullText = ScienceShip.Sector.System.StarType.ToString() + " star";
                            break;
                        default:
                            StarTypeFullText = ScienceShip.Sector.System.StarType.ToString();
                            break;
                    }

                    return string.Format(ResourceManager.GetString("SITREP_RESEARCH_SCIENCE_SHIP"),
                        ScienceShip.Name, ScienceShip.Sector, StarTypeFullText, _researchGained, StarTypeFullText);
                }
                else
                {
                    return string.Format(ResourceManager.GetString("SITREP_RESEARCH_SCIENCE_SHIP_RESULT_UNKNOWN"));
                }
            }
        }
    }

    [Serializable]
    public class ShipAssimilatedSitRepEntry : SitRepEntry
    {
        private readonly string _note;
        private readonly MapLocation _loc;

        public ShipAssimilatedSitRepEntry(Civilization owner, MapLocation loc, string Note) : base(owner, SitRepPriority.Purple)
        { _loc = loc; _note = Note; }

        public string Note => _note;
        public override SitRepCategory Categories => SitRepCategory.Military;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_loc];
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _note;
    }
    // End of SitRepEntry

    [Serializable]
    public class ShipDestroyedInWormholeSitRepEntry : SitRepEntry
    {
        private readonly MapLocation _wormholeLocation;
        public ShipDestroyedInWormholeSitRepEntry(Civilization owner, MapLocation wormholeLocation) : base(owner, SitRepPriority.Purple)
        { _wormholeLocation = wormholeLocation; }
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_wormholeLocation];
        public override SitRepCategory Categories => SitRepCategory.General;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_FLEET_DESTROYED_UNSTABLE_WORMHOLE"), _wormholeLocation);

    }
    // End of SitRepEntry

    [Serializable]
    public class ShipMedicalHelpProvidedSitRepEntry : SitRepEntry
    {
        private readonly MapLocation _location;
        private readonly string _note;
        public ShipMedicalHelpProvidedSitRepEntry(Civilization owner, MapLocation location, string note) : base(owner, SitRepPriority.Blue)
        { _location = location; _note = note; }
        //public string Note => _note;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_location];
        public override SitRepCategory Categories => SitRepCategory.General;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_SHIP_MEDICAL_HELP_PROVIDED"), _location, _note);

    }
    // End of SitRepEntry

    [Serializable]
    public class ShipStatusSitRepEntry : SitRepEntry
    {
        private readonly string _note;
        private readonly MapLocation _location;

        public ShipStatusSitRepEntry(Civilization owner, MapLocation location, string Note) : base(owner, SitRepPriority.Pink)
        { _location = location; _note = Note; }

        public string Note => _note;
        public override SitRepCategory Categories => SitRepCategory.Military;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.Universe.Map[_location];
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _note;

    }
    // End of SitRepEntry

    [Serializable]
    public class ShipSummarySitRepEntry : SitRepEntry
    {
        private readonly string _note;
        //private readonly MapLocation _loc;

        public ShipSummarySitRepEntry(Civilization owner, string Note) : base(owner, SitRepPriority.Purple)
        { /*_loc = loc;*/ _note = Note; }

        public string Note => _note;
        public override SitRepCategory Categories => SitRepCategory.Military;
        //public override SitRepAction Action => SitRepAction.CenterOnSector;
        //public override object ActionTarget => GameContext.Current.Universe.Map[_loc]; 
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => _note;

    }
    // End of SitRepEntry

    [Serializable]
    public class SupernovaSitRepEntry : SitRepEntry // not Supernovai
    {
        private readonly int _colonyID;
        public SupernovaSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Purple)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony missing for Supernova");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override bool HasDetails => true;  // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("SUPERNOVA_I_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("SUPERNOVA_I_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/Supernova.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SUPERNOVA_I_SUMMARY_TEXT"), Colony.Name, Colony.Location);

        public override bool IsPriority => true;

    }

    [Serializable]
    public class StarvationSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public StarvationSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Red)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_STARVATION"), Colony.Name, Colony.Location);
    }

    [Serializable]
    public class SystemAssaultSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        private readonly string _status;
        private readonly int _pop;
        private readonly string _newOwner;
        private readonly string _invaderUnitsDestroyed;
        private readonly string _defenderUnitsDestroyed;
        public SystemAssaultSitRepEntry(Civilization owner, Colony colony, string status, int pop, string newOwner, string invaderUnitsDestroyed, string defenderUnitsDestroyed)
            : base(owner, SitRepPriority.Purple)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony missing for SystemAssault");
            }

            _colonyID = colony.System.ObjectID;
            _status = status.ToUpper();
            _pop = pop;
            _newOwner = newOwner;  // maybe "new" Owner
            _invaderUnitsDestroyed = invaderUnitsDestroyed;
            _defenderUnitsDestroyed = defenderUnitsDestroyed;

        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_colonyID);
        //public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepAction Action => SitRepAction.CenterOnSector;

        public override object ActionTarget => System.Sector;
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("SYSTEMASSAULT_HEADER_TEXT")
                , System.Colony.Name, _status, _pop, _newOwner, _invaderUnitsDestroyed, _defenderUnitsDestroyed);
        public override string DetailText => string.Format(ResourceManager.GetString("SYSTEMASSAULT_DETAIL_TEXT")
                , System.Colony.Name, _status, _pop, _newOwner, _invaderUnitsDestroyed, _defenderUnitsDestroyed);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/SystemAssault.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SYSTEMASSAULT_SUMMARY_TEXT")
                , System.Colony.Name, _status, _pop, _newOwner, _invaderUnitsDestroyed, _defenderUnitsDestroyed);
        public override bool IsPriority => true;

    }
    // End of SitRepEntry


    [Serializable]
    public class TerroristBombingOfShipProductionSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public TerroristBombingOfShipProductionSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Purple)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("TERRORIST_BOMBING_OF_SHIP_PRODUCTION_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("TERRORIST_BOMBING_OF_SHIP_PRODUCTION_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/TerroristBombingOfShipProduction.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("TERRORIST_BOMBING_OF_SHIP_PRODUCTION_SUMMARY_TEXT"), Colony.Name, Colony.Location);
        public override bool IsPriority => true;

    }

    [Serializable]
    public class TerroristsCapturedSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public TerroristsCapturedSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Purple)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }

        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("TERRORISTS_CAPTURED_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("TERRORISTS_CAPTURED_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/TerroristsCaptured.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("TERRORISTS_CAPTURED_SUMMARY_TEXT"), Colony.Name, Colony.Location);
        public override bool IsPriority => true;

    }

    [Serializable]
    public class TradeGuildStrikesSitRepEntry : SitRepEntry
    {
        private readonly int _colonyID;
        public TradeGuildStrikesSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Purple)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);

        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("TRADE_GUILD_STRIKES_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("TRADE_GUILD_STRIKES_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/TradeGuildStrikes.png";
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("TRADE_GUILD_STRIKES_SUMMARY_TEXT"), Colony.Name, Colony.Location);
        public override bool IsPriority => true;

    }

    [Serializable]
    public class TradeRouteCreditsStolenAttackerSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly int _lostCredits;
        public TradeRouteCreditsStolenAttackerSitRepEntry(Civilization owner, Colony target, int lostCredits)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
            _lostCredits = lostCredits;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_TRADE_ROUTES_STOLEN_WORTH_SUCCESSFULLY"), _lostCredits, System.Name);
        //"We have stolen {0} worth of goods from the trade routes on {1}",
    }

    [Serializable]
    public class TradeRouteCreditsStolenTargetSitRepEntry : SitRepEntry
    {
        private readonly int _systemId;
        private readonly int _lostCredits;
        public TradeRouteCreditsStolenTargetSitRepEntry(Civilization owner, Colony target, int lostCredits)
            : base(owner, SitRepPriority.Red)
        {
            if (target == null)
            {
                throw new ArgumentNullException("colony");
            }

            _systemId = target.System.ObjectID;
            _lostCredits = lostCredits;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public override SitRepCategory Categories => SitRepCategory.ColonyStatus;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => System.Sector;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_TRADE_ROUTES_STOLEN_WORTH"), _lostCredits, System.Name);
        //"{0} credits worth of goods have been stolen from our trade routes on {1}",
    }

    [Serializable]
    public class TribblesSitRepEntry : SitRepEntry
    {
        public TribblesSitRepEntry(Civilization owner, Colony colony)
            : base(owner, SitRepPriority.Pink)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            _colonyID = colony.ObjectID;
        }
        private readonly int _colonyID;

        public Colony Colony => GameContext.Current.Universe.Get<Colony>(_colonyID);
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.ViewColony;
        public override object ActionTarget => Colony;
        public override bool HasDetails => true; // turn on/off for extra Dialog window
        public override string HeaderText => string.Format(ResourceManager.GetString("TRIBBLES_HEADER_TEXT"), Colony.Name, Colony.Location);
        public override string DetailText => string.Format(ResourceManager.GetString("TRIBBLES_DETAIL_TEXT"), Colony.Name, Colony.Location);
        public override string DetailImage => "vfs:///Resources/Images/ScriptedEvents/Tribbles.png";
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("TRIBBLES_SUMMARY_TEXT"), Colony.Name, Colony.Location);

    }

    [Serializable]
    public class UnassignedTradeRoute : SitRepEntry
    {
        private readonly TradeRoute _tradeRoute;
        private readonly int _systemId;
        public UnassignedTradeRoute(TradeRoute route) : base(route.SourceColony.Owner, SitRepPriority.Orange)
        {
            if (route == null)
            {
                throw new ArgumentException("TradeRoute");
            }

            _systemId = route.SourceColony.ObjectID;
            _tradeRoute = route;
        }
        public StarSystem System => GameContext.Current.Universe.Get<StarSystem>(_systemId);
        public TradeRoute TradeRoute => _tradeRoute;
        public override SitRepCategory Categories => SitRepCategory.SpecialEvent;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => TradeRoute.SourceColony.Sector;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_UNASSIGNED_TRADE_ROUTE"), TradeRoute.SourceColony, TradeRoute.SourceColony.Location);
    }

    [Serializable]
    public class WarDeclaredSitRepEntry : SitRepEntry
    {
        private readonly int _victimCivilizationID;
        private readonly int _aggressorCivilizationID;
        private readonly CivString _detailText;
        public WarDeclaredSitRepEntry(Civilization owner, Civilization aggressor, Civilization victim)
            : base(owner, SitRepPriority.Purple)
        {
            if (aggressor == null)
            {
                throw new ArgumentNullException("aggressor");
            }

            if (victim == null)
            {
                throw new ArgumentNullException("victim");
            }

            _victimCivilizationID = victim.CivID;
            _aggressorCivilizationID = aggressor.CivID;

            if (aggressor == owner || victim == owner)
            {
                _detailText = new CivString(
                    owner,
                    CivString.DiplomacyCategory,
                    owner == aggressor
                        ? "MESSAGE_SITREP_DETAILS_WAR_DECLARATION_US"
                        : "MESSAGE_SITREP_DETAILS_WAR_DECLARATION_THEM");
            }
            if (owner.Key == "BORG" && owner == aggressor)
            {
                _detailText = new CivString(owner, CivString.DiplomacyCategory, "MESSAGE_SITREP_RESISTANCE_IS_FUTILE");
            }
        }
        public override SitRepCategory Categories => SitRepCategory.Diplomacy | SitRepCategory.Military;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override object ActionTarget => GameContext.Current.CivilizationManagers[Victim.CivID].HomeSystem.Sector;
        public override bool IsPriority => true;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_WAR_DECLARED"), Aggressor.LongName, Victim.LongName);
        public override bool HasDetails => (Aggressor == Owner) || (Victim == Owner);   // turn on/off for extra Dialog window

        public override string DetailImage => (Owner == Aggressor) ? Victim.InsigniaPath : Aggressor.InsigniaPath;

        public override string DetailText => string.Format(_detailText.Value, Victim.LongName);

        public Civilization Victim => GameContext.Current.Civilizations[_victimCivilizationID];

        public Civilization Aggressor => GameContext.Current.Civilizations[_aggressorCivilizationID];

        public WarDeclaredSitRepEntry(Civilization owner, Civilization victim) : this(owner, owner, victim) { }
    }

    [Serializable]
    public class ViolateTreatySitRepEntry : SitRepEntry
    {
        private readonly int _ownerCivilizationID;
        private readonly int _victimCivilizationID;
        private readonly int _aggressorCivilizationID;
        private readonly CivString _detailText;
        public override SitRepCategory Categories => SitRepCategory.Diplomacy | SitRepCategory.Military;
        public override SitRepAction Action => SitRepAction.CenterOnSector;
        public override string SitRepComment { get; set; }
        public override string SummaryText => string.Format(ResourceManager.GetString("SITREP_NONAGGRESSION_TREATY_VIOLATION"), Aggressor.LongName);
        public override bool HasDetails => (Aggressor == OwnerCiv) || (Victim == OwnerCiv);   // turn on/off for extra Dialog window
        public override string DetailImage => Aggressor.InsigniaPath;
        public override string DetailText => string.Format(_detailText.Value, Aggressor.LongName);
        public override bool IsPriority => true;
        public Civilization Victim => GameContext.Current.Civilizations[_victimCivilizationID];
        public Civilization Aggressor => GameContext.Current.Civilizations[_aggressorCivilizationID];
        public Civilization OwnerCiv => GameContext.Current.Civilizations[_ownerCivilizationID];  // keyword owner already used for SitRep itself
        public ViolateTreatySitRepEntry(Civilization owner, Civilization victim) : this(owner, owner, victim) { }

        public ViolateTreatySitRepEntry(Civilization owner, Civilization aggressor, Civilization victim)
            : base(owner, SitRepPriority.Red)
        {
            if (aggressor == null)
            {
                throw new ArgumentNullException("aggressor");
            }

            if (victim == null)
            {
                throw new ArgumentNullException("victim");
            }

            _ownerCivilizationID = owner.CivID;
            _victimCivilizationID = victim.CivID;
            _aggressorCivilizationID = aggressor.CivID;

            if (aggressor == owner || victim == owner)
            {
                _detailText = new CivString(
                    owner,
                    CivString.DiplomacyCategory,
                    owner == aggressor
                        ? "MESSAGE_SITREP_DETAILS_NON_AGGRESSION_TREATY_US"
                        : "MESSAGE_SITREP_DETAILS_NON_AGGRESSION_TREATY_THEM");
            }
        }
    }
}
