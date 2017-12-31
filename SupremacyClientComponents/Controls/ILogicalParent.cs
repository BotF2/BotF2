namespace Supremacy.Client.Controls
{
    public interface ILogicalParent
    {
        void AddLogicalChild(object child);
        void RemoveLogicalChild(object child);
    }
}