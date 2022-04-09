using System;

namespace Treasured.UnitySdk
{
    public enum EmbedPosition
    {
        TopLeft,
        TopRight,
        [Obsolete] // Keep the property there so that serializedProperty.enumValueIndex stays the same
        BottomLeft,
        [Obsolete]
        BottomRight,
        Center,
        Fullscreen,
        NewTab
    }
}
