// Officer.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using System.Collections.Generic;

using System.ComponentModel;
using System.Runtime.Serialization;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Types;

using System.Linq;

using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    public enum AgentStatus
    {
        Unassigned,
        AvailableForReassignment,
        UnavailableForReassignment
    }

    [Serializable]
    public class Agent : GameObject, IGameTurnListener
    {
        public const int NaturalSkillsPerAgent = 3;

        private short _ownerId = (short)Civilization.InvalidID;
        private int _appearanceTurn;
        private string _profileName;
        private AgentSkillMeters _skillMeters;
        private AgentAssignment _assignment;
        private MapLocation? _currentLocation;
        //private AgentCareer _career;

        [NonSerialized]
        private Lazy<AgentProfile> _profile;

        private Mission _mission;

        public Agent([NotNull] AgentProfile profile, [NotNull] Civilization owner)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            if (owner == null)
                throw new ArgumentNullException("owner");

            _profile = new Lazy<AgentProfile>(LazyLoadProfile);

            Initialize(profile, owner);
        }

        public Agent(GameObjectID objectId, AgentProfile profile, Civilization owner)
            : base(objectId)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            if (owner == null)
                throw new ArgumentNullException("owner");

            _profile = new Lazy<AgentProfile>(LazyLoadProfile);

            Initialize(profile, owner);
        }

        public Agent()
        {
            // For cloning purposes only...
            _profile = new Lazy<AgentProfile>(LazyLoadProfile);
        }

        public int TurnsSinceAppearance
        {
            get { return GameContext.Current.TurnNumber - _appearanceTurn; }
        }
        
        private AgentProfile LazyLoadProfile()
        {
            return GameContext.Current.AgentDatabase[Owner][_profileName];
        }

        private void Initialize([NotNull] AgentProfile profile, [NotNull] Civilization owner)
        {
            _profileName = profile.Name;
            _ownerId = (short)owner.CivID;
            _appearanceTurn = GameContext.Current.TurnNumber;

            var naturalSkills = NaturalSkillsFromProfile(profile);

            _skillMeters = new AgentSkillMeters(naturalSkills[0], naturalSkills[1], naturalSkills[2]);

            _skillMeters[0].Reset(RandomProvider.Shared.Next(15, 21));
            _skillMeters[1].Reset(RandomProvider.Shared.Next(10, 15));
            _skillMeters[2].Reset(RandomProvider.Shared.Next(3, 8));

            _skillMeters.ForEach(o => o.PropertyChanged += OnSkillMetersPropertyChanged);

            Mission.CreateDefaultMission(this).Assign(this);
        }

        public MapLocation? CurrentLocation
        {
            get { return _currentLocation; }
            internal set
            {
                _currentLocation = value;
                OnPropertyChanged("CurrentLocation");
            }
        }

        public GameObjectID OwnerID
        {
            get { return _ownerId; }
            set
            {
                if (value < 0)
                    value = Civilization.InvalidID;

                _ownerId = (short)value;

                OnPropertyChanged("OwnerID");
                OnPropertyChanged("Owner");
            }
        }

        [NotNull]
        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        //public AgentCareer Career
        //{
        //    get { return _career; }
        //    internal set { _career = value; }
        //}

        [NotNull]
        public AgentProfile Profile
        {
            get { return _profile.Value; }
            set
            {
                _profile = new Lazy<AgentProfile>(() => value);
                OnPropertyChanged("Profile");
            }
        }

        [NotNull]
        public AgentSkillMeter PrimarySkillMeter
        {
            get { return _skillMeters[0]; }
        }

        [NotNull]
        public AgentSkillMeter SecondarySkillMeter
        {
            get { return _skillMeters[1]; }
        }

        [NotNull]
        public AgentSkillMeter TertiarySkillMeter
        {
            get { return _skillMeters[2]; }
        }

        [NotNull]
        public AgentSkillMeters SkillMeters
        {
            get
            {
                if (_skillMeters == null)
                {
                    var profile = Profile;
                    var naturalSkills = NaturalSkillsFromProfile(profile);

                    _skillMeters = new AgentSkillMeters(
                        naturalSkills[0],
                        naturalSkills[1],
                        naturalSkills[2]);
                }

                return _skillMeters;
            }
        }

        [CanBeNull]
        public AgentAssignment Assignment
        {
            get { return _assignment; }
            internal set
            {
                _assignment = value;
                OnPropertyChanged("Assignment");
            }
        }

        public bool HasMission
        {
            get { return (_mission != null) && !(_mission is NullMission); }
        }

        public bool IsAvailableForMission
        {
            get { return !HasMission; }
        }

        [CanBeNull]
        public Mission Mission
        {
            get { return _mission; }
            internal set
            {
                _mission = value;

                OnPropertyChanged("Mission");
                OnPropertyChanged("HasMission");
                OnPropertyChanged("IsAvailableForMission");
                OnPropertyChanged("Status");
            }
        }

        public AgentStatus Status
        {
            get
            {
                var mission = Mission;
                if (mission == null || mission is NullMission)
                    return AgentStatus.Unassigned;

                if (mission.CurrentPhase.AreAssignmentsChangesAllowed || mission.CurrentPhase.IsMissionCancellationAllowed)
                    return AgentStatus.AvailableForReassignment;

                return AgentStatus.UnavailableForReassignment;
            }
        }

        private void OnSkillMetersPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged("SkillMeters");
        }

        #region IOwnedDataSerializableAndRecreatable Members
        public override void DeserializeOwnedData([NotNull] SerializationReader reader, [CanBeNull] object context)
        {
            base.DeserializeOwnedData(reader, context);

            _ownerId = reader.ReadInt16();
            _profileName = reader.ReadString();
            _skillMeters = reader.Read<AgentSkillMeters>();

            if (_profile == null)
                _profile = new Lazy<AgentProfile>(LazyLoadProfile);
        }

        public override void SerializeOwnedData([NotNull] SerializationWriter writer, [CanBeNull] object context)
        {
            base.SerializeOwnedData(writer, context);
            
            writer.Write(_ownerId);
            writer.Write(_profileName);
            writer.WriteObject(_skillMeters);
        }
        #endregion

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_profile == null)
                _profile = new Lazy<AgentProfile>(LazyLoadProfile);
        }

        protected override Cloneable CreateInstance(ICloneContext context)
        {
            return new Agent();
        }

        public override void CloneFrom(Cloneable source, ICloneContext context)
        {
            var typedSource = (Agent)source;
            
            base.CloneFrom(source, context);

            _ownerId = typedSource._ownerId;
            _profileName = typedSource._profileName;
            _mission = typedSource._mission;

            if (_profile == null)
                _profile = new Lazy<AgentProfile>(LazyLoadProfile);

            if (_skillMeters == null)
            {
                _skillMeters = new AgentSkillMeters(
                    Clone(typedSource.PrimarySkillMeter),
                    Clone(typedSource.SecondarySkillMeter),
                    Clone(typedSource.TertiarySkillMeter));
            }
            else
            {
                _skillMeters.Keys.ForEach(o => _skillMeters[o].CloneFrom(typedSource._skillMeters[o], context));
            }
        }

        private static IIndexedCollection<AgentSkill> NaturalSkillsFromProfile(AgentProfile profile)
        {
            var naturalSkills = profile.NaturalSkills;
            var result = new CollectionBase<AgentSkill>();

            for (var i = 0; i < naturalSkills.Count && result.Count < 3; i++)
            {
                if (!result.Contains(naturalSkills[i]))
                    result.Add(naturalSkills[i]);
            }

            if (result.Count == 3)
                return result;

            var remainingSkills = new List<AgentSkill>(EnumHelper.GetValues<AgentSkill>().Except(result));

            remainingSkills.RandomizeInPlace(RandomProvider.Shared);

            for (var i = 0; i < remainingSkills.Count && result.Count < 3; i++)
                result.Add(remainingSkills[i]);

            return result;
        }

        #region Implementation of IGameTurnListener

        void IGameTurnListener.OnTurnStarted(GameContext game)
        {
            var assignment = Mission as IGameTurnListener;
            if (assignment != null)
                assignment.OnTurnStarted(game);
        }

        void IGameTurnListener.OnTurnPhaseStarted(GameContext game, TurnPhase phase)
        {
            var assignment = Mission as IGameTurnListener;
            if (assignment != null)
                assignment.OnTurnPhaseStarted(game, phase);
        }

        void IGameTurnListener.OnTurnPhaseFinished(GameContext game, TurnPhase phase)
        {
            var assignment = Mission as IGameTurnListener;
            if (assignment != null)
                assignment.OnTurnPhaseFinished(game, phase);
        }

        void IGameTurnListener.OnTurnFinished(GameContext game)
        {
            var assignment = Mission as IGameTurnListener;
            if (assignment != null)
                assignment.OnTurnFinished(game);

            OnPropertyChanged("TurnsSinceAppearance");
        }

        #endregion
    }
}