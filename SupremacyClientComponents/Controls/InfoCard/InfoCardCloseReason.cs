namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Indicates the reason why a <see cref="IInfoCardWindow"/> was last closed.
    /// </summary>
    public enum InfoCardCloseReason
    {
        /// <summary>
        /// Closed because the window was rafting at the time the owner <see cref="InfoCardSite"/> was unloaded.
        /// </summary>
        InfoCardSiteUnloaded,

        /// <summary>
        /// The layout was cleared, probably in preparation for another layout load.
        /// </summary>
        LayoutCleared,

        /// <summary>
        /// The ancestor info InfoCard window was closed.
        /// </summary>
        InfoCardWindowClosed,

        /// <summary>
        /// Closed for an other reason, possibly by the end user or via a programmatic call to <c>Close</c>.
        /// </summary>
        Other,
    }
}