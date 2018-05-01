using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public static class TransformerUtilities
    {
        public static NotificationLevel ToNotificationLevelV2(this NotificationLevelVersionOne value)
        {
            switch (value)
            {
                case NotificationLevelVersionOne.Error:
                    return NotificationLevel.Error;
                case NotificationLevelVersionOne.Note:
                    return NotificationLevel.Note;
                default:
                    return NotificationLevel.Warning;
            }
        }
    }
}
