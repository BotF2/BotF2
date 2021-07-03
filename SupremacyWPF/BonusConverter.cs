// BonusConverter.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Supremacy.Buildings;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Utility;

namespace Supremacy.Client
{
    [ValueConversion(typeof(Bonus), typeof(string))]
    public class BonusDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Bonus ? BonusDescriptions.GetDescription((Bonus)value) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(BonusType), typeof(string))]
    public class BonusTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return BonusDescriptions.GetDescription((BonusType)value);
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(IEnumerable<Bonus>), typeof(string)),
     ValueConversion(typeof(TechObjectDesign), typeof(string))]
    public class BonusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder sb = new StringBuilder();
            bool commaSeparated = parameter != null;
            if (value is IEnumerable<Bonus>)
            {
                List<Bonus> bonuses = new List<Bonus>((IEnumerable<Bonus>)value);
                for (int i = 0; i < bonuses.Count; i++)
                {
                    if (i > 0)
                    {
                        _ = commaSeparated ? sb.Append(", ") : sb.Append("\n");
                    }
                    _ = sb.Append(BonusDescriptions.GetDescription(bonuses[i]));
                }

            }
            else if (value is ShipyardDesign)
            {
                _ = sb.Append(ResourceManager.GetString("SHIPYARD_BONUS_BUILDS_SHIPS"));
            }
            else if (value is BuildingDesign bDesign)
            {
                for (int i = 0; i < bDesign.Bonuses.Count; i++)
                {
                    if (i > 0)
                    {
                        _ = sb.Append(commaSeparated ? ", " : "\n");
                    }

                    _ = sb.Append(BonusDescriptions.GetDescription(bDesign.Bonuses[i]));
                }
            }
            else if (value is ProductionFacilityDesign pfDesign)
            {
                BonusType bonusType;

                switch (pfDesign.Category)
                {
                    default:
                    case ProductionCategory.Food:
                        bonusType = BonusType.Food;
                        break;
                    case ProductionCategory.Industry:
                        bonusType = BonusType.Industry;
                        break;
                    case ProductionCategory.Energy:
                        bonusType = BonusType.Energy;
                        break;
                    case ProductionCategory.Research:
                        bonusType = BonusType.Research;
                        break;
                    case ProductionCategory.Intelligence:
                        bonusType = BonusType.Intelligence;
                        break;
                }

                Bonus bonus = new Bonus(bonusType, pfDesign.UnitOutput);

                _ = sb.Append(BonusDescriptions.GetDescription(bonus));
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(BuildRestriction), typeof(string))]
    public class BuildRestrictionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BuildRestriction? buildRestriction = value as BuildRestriction?;
            if (buildRestriction == null)
            {
                return string.Empty;
            }

            if (buildRestriction == BuildRestriction.None)
            {
                return BuildRestrictionDescriptions.GetDescription(buildRestriction.Value);
            }

            StringBuilder sb = new StringBuilder();
            bool commaSeparated = parameter != null;

            foreach (BuildRestriction restriction in EnumHelper.GetValues<BuildRestriction>())
            {
                if (restriction == BuildRestriction.None || (buildRestriction & restriction) != restriction)
                {
                    continue;
                }

                if (sb.Length != 0)
                {
                    _ = commaSeparated ? sb.Append(", ") : sb.Append("\n");
                }

                _ = sb.Append(BuildRestrictionDescriptions.GetDescription(restriction));
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(object), typeof(string))]
    public sealed class WholeCurrencyFormatConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0:#,0}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (decimal.TryParse(value.ToString(), out decimal number))
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                    if (converter.CanConvertFrom(typeof(decimal)))
                    {
                        return converter.ConvertFrom(number);
                    }
                }
            }
            return value;
        }

        #endregion
    }

    [ValueConversion(typeof(object), typeof(object))]
    public class BuildableDesignConverter : AppContextAwareValueConverter
    {
        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<TechObjectDesign> designs = value as IEnumerable<TechObjectDesign> ?? Enumerable.Empty<TechObjectDesign>();
            try
            {
                Game.CivilizationManager localPlayerEmpire = AppContext.LocalPlayerEmpire;
                HashSet<TechObjectDesign> techTree = localPlayerEmpire.TechTree.ToHashSet();

                foreach (Entities.Civilization memberCivilization in DiplomacyHelper.GetMemberCivilizations(localPlayerEmpire.Civilization))
                {
                    TechTree memberTechTree = AppContext.CurrentGame.TechTrees[memberCivilization];
                    if (memberTechTree != null)
                    {
                        techTree.UnionWith(memberTechTree);
                    }
                }

                return designs.Where(techTree.Contains);
            }
            catch
            {
                return designs;
            }
        }
        #endregion
    }
}
