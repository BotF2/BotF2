using System;
using System.Collections.Generic;
using System.Windows.Markup;

using Supremacy.Annotations;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Scripting
{
    [UsedImplicitly]
    [ContentProperty("Options")]
    [DictionaryKeyProperty("EventID")]
    public sealed class EventDefinition : SupportInitializeBase
    {
        private readonly Dictionary<string, object> _options= new Dictionary<string, object>();
        private string _description;

        public string EventID { get; set; }
        public Type EventType { get; set; }

        public string Description
        {
            get
            {
                var description = _description;
                if (!string.IsNullOrWhiteSpace(description))
                    return description;

                var eventId = EventID;
                var eventType = EventType;

                if (eventType != null)
                {
                    if (!string.IsNullOrWhiteSpace(eventId))
                        return string.Format("{0} ({1})", eventId, eventType.Name);

                    return eventType.Name;
                }

                if (!string.IsNullOrWhiteSpace(eventId))
                    return eventId;

                return "(Unknown Event)";
            }
            set { _description = value; }
        }

        public Dictionary<string, object> Options => _options;

        protected override void EndInitCore()
        {
            if (string.IsNullOrWhiteSpace(EventID))
            {
                var description = _description;
                if (string.IsNullOrEmpty(description))
                    GameLog.Client.GameData.Error("Error in ScriptedEventDatabase: Event must specify a unique event ID.");
                else
                    GameLog.Client.GameData.ErrorFormat("Error in ScriptedEventDatabase: Event \"{0}\" must specify a unique event ID.", description);
            }

            if (EventType == null)
            {
                var description = _description;

                GameLog.Client.GameData.ErrorFormat(
                    "Error in ScriptedEventDatabase: Event \"{0}\" must declare an EventType.",
                    string.IsNullOrWhiteSpace(description) ? EventID : description);
            }

            base.EndInitCore();
        }
    }
}