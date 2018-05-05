using System;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Types;

using System.Linq;

using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public class AgentPool : Cloneable, IOwnedDataSerializableAndRecreatable
    {
        private GameObjectID _ownerId;
        private AgentCollection _currentAgents;
        private CollectionBase<FutureAgent> _futureAgents;
        private CollectionBase<string> _unusedAgents;
        //private IList<string> _agentSkillIncrementer;

        public AgentPool([NotNull] Civilization owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            _ownerId = owner.CivID;
            _currentAgents = new AgentCollection();
            _futureAgents = new CollectionBase<FutureAgent>();
            _unusedAgents = new CollectionBase<string>();

            AgentProfileCollection agents;

            if (GameContext.Current.AgentDatabase.TryGetValue(owner, out agents))
            {
                _unusedAgents = new CollectionBase<string>(agents.Count);
                agents.Select(o => o.Name).CopyTo(_unusedAgents);
            }
            else
            {
                _unusedAgents = new CollectionBase<string>();
            }
        }
        //public void AgentSkillIncrementer([NotNull] Civilization owner)
        //{
        //    if (owner == null)
        //        throw new ArgumentNullException("owner");

        //    _ownerId = owner.CivID;
        //    _currentAgents = new AgentCollection();
        //    _futureAgents = new CollectionBase<FutureAgent>();
        //    _unusedAgents = new CollectionBase<string>();

        //    AgentProfileCollection agents;
        //    if (GameContext.Current.AgentDatabase.TryGetValue(owner, out agents))
        //    {
        //        GameLog.Client.GameData.DebugFormat("AgentPool.cs: agents: {0}", agents );
        //        //agents.Select(o => o.Name).CopyTo(_agentSkillIncrementer);
        //        //agents.Select(o => o.Name).CopyTo(_unusedAgents);

        //    }
        //}
        //public void Add(AgentSkillRatingsSnapshot ratings, [NotNull] Civilization owner)
        //{
        //    if (owner == null)
        //        throw new ArgumentNullException("owner");

        //    _ownerId = owner.CivID;
        //    _currentAgents = new AgentCollection();
        //    _futureAgents = new CollectionBase<FutureAgent>();
        //    _unusedAgents = new CollectionBase<string>();

        //    AgentProfileCollection agents;
        //    if (GameContext.Current.AgentDatabase.TryGetValue(owner, out agents))
        //    {
        //        if (ratings == null)
        //            throw new ArgumentNullException("ratings");

        //        EnumHelper
        //            .GetValues<AgentSkill>()
        //            .ForEach(
        //                skill =>
        //                {
        //                    AgentSkillMeter meter;

        //                    if (TryGetValue(skill, out meter))
        //                        meter.AdjustCurrent(ratings[skill]);
        //                });
        //    }
        //}
        //public bool TryGetValue(AgentSkill key, out AgentSkillMeter value)
        //{
        //    if (key == _primaryMeter.Skill)
        //    {
        //        value = _primaryMeter;
        //        return true;
        //    }

        //    if (key == _secondaryMeter.Skill)
        //    {
        //        value = _secondaryMeter;
        //        return true;
        //    }

        //    if (key == _tertiaryMeter.Skill)
        //    {
        //        value = _tertiaryMeter;
        //        return true;
        //    }

        //    value = null;
        //    return false;
        //}
        private AgentPool()
        {
            // For cloning purposes only...
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public AgentCollection CurrentAgents
        {
            get { return _currentAgents; }
        }

        public CollectionBase<FutureAgent> FutureAgents
        {
            get { return _futureAgents; }
        }

        public void Update()
        {
            //TODO Re-enable when more dev is spent on Agents, right now they don't do nothing besides creating bugs
            RecruitNewAgents();
            ScheduleFutureAgents();
        }

        private void RecruitNewAgents()
        {
            var newRecruits =
                (
                    from o in _futureAgents
                    where o.AppearanceTurnNumber == GameContext.Current.TurnNumber
                    select o
                ).ToList();

            var owner = Owner;
            var civManager = GameContext.Current.CivilizationManagers[owner];

            while (newRecruits.Count != 0)
            {
                var agent = newRecruits[0].Profile.Spawn(owner);
                _currentAgents.Add(agent);
                newRecruits.RemoveAt(0);

                civManager.SitRepEntries.Add(new NewAgentSitRepEntry(agent));
            }
        }

        private void ScheduleFutureAgents()
        {
            var maxActiveAgents = PersonnelConstants.Instance.MaxActiveAgentsPerEmpire;
            var currentAgentCount = _currentAgents.Count + _futureAgents.Count;

            if (currentAgentCount >= maxActiveAgents)
                return;

            var owner = Owner;

            AgentProfileCollection agents;

            if (!GameContext.Current.AgentDatabase.TryGetValue(owner, out agents))
                return;

            var civManager = GameContext.Current.CivilizationManagers[owner];
            var researchFields = GameContext.Current.ResearchMatrix.Fields;
            var averageTechLevel = researchFields.Average(o => civManager.Research.GetTechLevel(o));

            var availableAgents =
                (
                    from o in _unusedAgents.Select(name => agents[name])
                    let minTechLevelBeforeAppearance = o.MinTechLevelBeforeAppearance
                    let maxTechLevelBeforeAppearance = (o.MaxTechLevelBeforeAppearance == 0)
                                                           ? int.MaxValue
                                                           : o.MaxTechLevelBeforeAppearance
                    where averageTechLevel >= minTechLevelBeforeAppearance &&
                          averageTechLevel <= maxTechLevelBeforeAppearance
                    orderby minTechLevelBeforeAppearance ascending,
                            maxTechLevelBeforeAppearance ascending
                    select o
                ).ToList();

            var nextRecruitTurn = GameContext.Current.TurnNumber;
            var minTurnsBetweenRecruits = PersonnelConstants.Instance.MinTurnsBetweenAgentRecruitment;

            while (currentAgentCount < maxActiveAgents &&
                   availableAgents.Count != 0)
            {
                nextRecruitTurn += RandomProvider.Shared.Next(
                    minTurnsBetweenRecruits,
                    minTurnsBetweenRecruits * 2);

                var recruitProfile = availableAgents[0];

                var newRecruit = new FutureAgent(
                    nextRecruitTurn,
                    owner,
                    recruitProfile);

                availableAgents.RemoveAt(0);

                _futureAgents.Add(newRecruit);

                //if (GameLog.Client.GameData.IsDebugEnabled)
                //{
                //    GameLog.Client.GameData.DebugFormat(
                //        "Scheduling '{0}' agent '{1}' to appear on turn {2}...",
                //        owner.ShortName,
                //        recruitProfile.Name,
                //        newRecruit.AppearanceTurnNumber);
                //}
            }
            GameLog.Client.GameData.DebugFormat("Scheduling Agents is done !");
        }

        #region Dependent Type: FutureAgent

        [Serializable]
        public class FutureAgent : IOwnedDataSerializableAndRecreatable
        {
            private int _appearanceTurnNumber;
            private GameObjectID _ownerId;
            private string _profileName;

            private Lazy<AgentProfile> _profile;

            public FutureAgent(int appearanceTurnNumber, [NotNull] Civilization owner, [NotNull] AgentProfile profile)
            {
                if (owner == null)
                    throw new ArgumentNullException("owner");
                if (profile == null)
                    throw new ArgumentNullException("profile");

                if (appearanceTurnNumber == 0)
                    appearanceTurnNumber = 1;

                _appearanceTurnNumber = appearanceTurnNumber;
                _ownerId = owner.CivID;
                _profileName = profile.Name;

                _profile = new Lazy<AgentProfile>(LazyLoadProfile);
            }

            private AgentProfile LazyLoadProfile()
            {
                return GameContext.Current.AgentDatabase[Owner][_profileName];
            }

            public int AppearanceTurnNumber
            {
                get { return _appearanceTurnNumber; }
            }

            public Civilization Owner
            {
                get { return GameContext.Current.Civilizations[_ownerId]; }
            }

            public AgentProfile Profile
            {
                get { return _profile.Value; }
            }

            #region Implementation of IOwnedDataSerializable
            void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
            {
                _appearanceTurnNumber = reader.ReadInt32();
                _ownerId = reader.ReadInt32();
                _profileName = reader.ReadString();

                if (_profile == null)
                    _profile = new Lazy<AgentProfile>(LazyLoadProfile);
            }

            void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
            {
                writer.Write(_appearanceTurnNumber);
                writer.Write(_ownerId);
                writer.Write(_profileName);
            }
            #endregion
        }

        #endregion

        #region Overrides of Cloneable
        protected override Cloneable CreateInstance(ICloneContext context)
        {
            return new AgentPool();
        }

        public override void CloneFrom(Cloneable source, ICloneContext context)
        {
            var typedSource = (AgentPool)source;

            base.CloneFrom(typedSource, context);

            _ownerId = typedSource._ownerId;
            _currentAgents = new AgentCollection();
            _futureAgents = new CollectionBase<FutureAgent>();

            _currentAgents.AddRange(typedSource._currentAgents.Select(o => Clone(o, context)));
            _futureAgents.AddRange(typedSource._futureAgents);
        }
        #endregion

        #region Implementation of IOwnedDataSerializable
        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _ownerId = reader.ReadInt32();
            _currentAgents = reader.Read<AgentCollection>();
            _futureAgents = reader.Read<CollectionBase<FutureAgent>>();
            _unusedAgents = reader.Read<CollectionBase<string>>();
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_ownerId);
            writer.WriteObject(_currentAgents);
            writer.WriteObject(_futureAgents);
            writer.WriteObject(_unusedAgents);
        }
        #endregion
    }
}