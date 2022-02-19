// BonusDescription.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.IO;

using Supremacy.Data;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Utility;

namespace Supremacy.Economy
{
    /// <summary>
    /// Helper class for getting the text descriptions of building bonuses.
    /// </summary>
    public static class BonusDescriptions
    {
        /// <summary>
        /// The table containing the bonus descriptions text.
        /// </summary>
        private static readonly Table _descriptions;

        /// <summary>
        /// Initializes the <see cref="BonusDescriptions"/> class.
        /// </summary>
        static BonusDescriptions()
        {
            string path = ResourceManager.GetResourcePath(@"Resources\Data\BonusDescriptions.txt");
            _descriptions = new Table(Table<string>.ReadFromStream(new StreamReader(path)));
        }

        /// <summary>
        /// Gets the text description of a specific bonus type.
        /// </summary>
        /// <param name="bonus">The bonus type.</param>
        /// <returns>The description.</returns>
        public static string GetDescription(BonusType bonus)
        {

            if (!_descriptions.TryGetValue(bonus.ToString(), 0, out string description))
            {
                return string.Format("{0}", bonus);
            }

            if (!_descriptions.TryGetValue(bonus.ToString(), 1, out string isPercent) || !StringHelper.IsTrue(isPercent))
            {
                return string.Format("{0}", description);
            }

            return string.Format("% {0}", description);
        }

        /// <summary>
        /// Gets the text description of a specific bonus.
        /// </summary>
        /// <param name="bonus">The bonus.</param>
        /// <returns>The description.</returns>
        public static string GetDescription(Bonus bonus)
        {

            if (!_descriptions.TryGetValue(bonus.BonusType.ToString(), 0, out string description))
            {
                return string.Format("{0:+#,0;-#,0} {1}", bonus.Amount, bonus.BonusType);
            }

            if (!_descriptions.TryGetValue(bonus.BonusType.ToString(), 1, out string isPercent) || !StringHelper.IsTrue(isPercent))
            {
                return string.Format("{0:+#,0;-#,0} {1}", bonus.Amount, description);
            }

            return string.Format("{0:+#,0;-#,0}% {1}", bonus.Amount, description);
        }
    }

    /// <summary>
    /// Helper class for getting the text descriptions of building restrictions.
    /// </summary>
    public static class BuildRestrictionDescriptions
    {
        /// <summary>
        /// The table containing the build restrictions text.
        /// </summary>
        private static readonly Table _descriptions;

        /// <summary>
        /// Initializes the <see cref="BuildRestrictionDescriptions"/> class.
        /// </summary>
        static BuildRestrictionDescriptions()
        {
            _descriptions = GameContext.Current.Tables.EnumTables["BuildRestriction"];
        }

        /// <summary>
        /// Gets the text description of a specific build restriction.
        /// </summary>
        /// <param name="restriction">The build restriction.</param>
        /// <returns>The description.</returns>
        public static string GetDescription(BuildRestriction restriction)
        {
            return _descriptions.TryGetValue(restriction.ToString(), 0, out string description) ? description : restriction.ToString();
        }
    }
}
