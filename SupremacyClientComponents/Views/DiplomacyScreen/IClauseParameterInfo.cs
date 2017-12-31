namespace Supremacy.Client.Views
{
    public interface IClauseParameterInfo
    {
        bool IsParameterValid { get; }
        object GetParameterData();
    }
}