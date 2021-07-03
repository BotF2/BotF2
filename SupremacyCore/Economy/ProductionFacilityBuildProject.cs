// 
// ProductionFacilityBuildProject.cs
// 
// Copyright (c) 2011-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace Supremacy.Economy
{
    [Serializable]
    public class ProductionFacilityBuildProject : BuildProject
    {
        private readonly int _colonyId;
        private readonly int _designId;

        public Colony Source => GameContext.Current.Universe.Objects[_colonyId] as Colony;

        public ProductionFacilityDesign FacilityDesign => GameContext.Current.TechDatabase.ProductionFacilityDesigns[_designId];

        protected override int GetIndustryAvailable()
        {
            return Source.GetProductionOutput(ProductionCategory.Industry);
        }

        public ProductionFacilityBuildProject(
            Colony colony,

            ProductionFacilityDesign target)

            : base(colony.Owner, colony, target)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            _designId = target.DesignID;
            _colonyId = colony.ObjectID;
        }

        public override BuildProject CloneEquivalent()
        {
            return new ProductionFacilityBuildProject(Source, FacilityDesign);
        }
    }

    [Serializable]
    public class OrbitalBatteryBuildProject : BuildProject
    {
        [NonSerialized] private Lazy<Colony> _colony;

        public Colony Source => _colony.Value;

        public OrbitalBatteryDesign OrbitalBatteryDesign => BuildDesign as OrbitalBatteryDesign;

        protected override int GetIndustryAvailable()
        {
            return Source.GetProductionOutput(ProductionCategory.Industry);
        }

        public OrbitalBatteryBuildProject(Colony colony, OrbitalBatteryDesign target)
            : base(colony.Owner, colony, target)
        {
            if (colony == null)
            {
                throw new ArgumentNullException("colony");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            _colony = new Lazy<Colony>(FindColony, LazyThreadSafetyMode.PublicationOnly);
        }

        private Colony FindColony()
        {
            return GameContext.Current.Universe.Map[Location].System.Colony;
        }

        public override BuildProject CloneEquivalent()
        {
            return new OrbitalBatteryBuildProject(Source, OrbitalBatteryDesign);
        }

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);

            _colony = new Lazy<Colony>(FindColony, LazyThreadSafetyMode.PublicationOnly);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _colony = new Lazy<Colony>(FindColony, LazyThreadSafetyMode.PublicationOnly);
        }
    }

    [Serializable]
    public class ProductionFacilityUpgradeProject : ProductionFacilityBuildProject
    {
        private readonly int _baseTypeId;

        public override string Description => string.Format(
                    ResourceManager.GetString("UPGRADE_FORMAT_STRING"),
                    ResourceManager.GetString(FacilityDesign.Name));

        protected ProductionFacilityDesign BaseFacilityType => GameContext.Current.TechDatabase.ProductionFacilityDesigns[_baseTypeId];

        protected override int IndustryRequired
        {
            get
            {
                int count = Source.GetTotalFacilities(BaseFacilityType.Category);
                int unitCost = (int)(0.50 * base.IndustryRequired);
                return count * unitCost;
            }
        }

        protected override ResourceValueCollection ResourcesRequired => new ResourceValueCollection();

        public override bool IsUpgrade => true;

        public override void Finish()
        {
            Source.SetFacilityType(FacilityDesign.Category, FacilityDesign);

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[Builder];
            if (civManager == null)
            {
                return;
            }

            civManager.SitRepEntries.Add(new ItemBuiltSitRepEntry(Builder, BuildDesign, Location));
        }

        public ProductionFacilityUpgradeProject(Colony colony, ProductionFacilityDesign target)
            : base(colony, target)
        {
            _baseTypeId = colony.GetFacilityType(target.Category).DesignID;
        }

        public override BuildProject CloneEquivalent()
        {
            return new ProductionFacilityUpgradeProject(Source, FacilityDesign);
        }
    }

    [Serializable]
    public class OrbitalBatteryUpgradeProject : OrbitalBatteryBuildProject
    {
        private readonly int _baseTypeId;
        private readonly ResourceValueCollection _resourcesRequired;

        public override string Description => string.Format(
                    ResourceManager.GetString("UPGRADE_FORMAT_STRING"),
                    ResourceManager.GetString(OrbitalBatteryDesign.Name));

        protected OrbitalBatteryDesign BaseOrbitalBatteryDesign => GameContext.Current.TechDatabase.OrbitalBatteryDesigns[_baseTypeId];

        protected override int IndustryRequired
        {
            get
            {
                int count = Source.TotalOrbitalBatteries;
                int unitCost = (int)(0.50 * base.IndustryRequired);
                return count * unitCost;
            }
        }

        protected override ResourceValueCollection ResourcesRequired => _resourcesRequired;

        public override bool IsUpgrade => true;

        public override void Finish()
        {
            CivilizationManager civManager = GameContext.Current.CivilizationManagers[Builder];

            Source.OrbitalBatteryDesign = OrbitalBatteryDesign;

            if (civManager != null)
            {
                civManager.SitRepEntries.Add(
                    new ItemBuiltSitRepEntry(Builder, BuildDesign, Location));
            }
        }

        public OrbitalBatteryUpgradeProject(Colony colony, OrbitalBatteryDesign target)
            : base(colony, target)
        {
            _baseTypeId = colony.OrbitalBatteryDesign.DesignID;

            ResourceValueCollection resourcesRequired = new ResourceValueCollection();

            foreach (ResourceType resourceType in EnumHelper.GetValues<ResourceType>())
            {
                resourcesRequired[resourceType] = (int)Math.Ceiling(base.ResourcesRequired[resourceType] / 2d);
            }

            _resourcesRequired = resourcesRequired;
        }

        public override BuildProject CloneEquivalent()
        {
            return new OrbitalBatteryUpgradeProject(Source, OrbitalBatteryDesign);
        }
    }
}
