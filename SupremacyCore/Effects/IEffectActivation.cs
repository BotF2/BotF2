using System;

using Supremacy.Annotations;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Effects
{
    public interface IEffectActivation
    {
        bool HasDescription { get; }
        string DescriptionExpression { get; }
        string ValueExpression { get; }
    }

    [Serializable]
    public class EffectActivation : SupportInitializeBase, IEffectActivation
    {
        private string _descriptionExpression;
        private string _valueExpression;

        public EffectActivation([CanBeNull] string descriptionExpression, [NotNull] string valueExpression)
        {
            Guard.ArgumentNotNullOrWhiteSpace(valueExpression, "valueExpression");

            DescriptionExpression = descriptionExpression;
            ValueExpression = valueExpression;
        }

        public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionExpression);

        public string DescriptionExpression
        {
            get { return _descriptionExpression; }
            set
            {
                VerifyInitializing();
                _descriptionExpression = value;
            }
        }

        public string ValueExpression
        {
            get { return _valueExpression; }
            set
            {
                VerifyInitializing();
                _valueExpression = value;
            }
        }
    }
}