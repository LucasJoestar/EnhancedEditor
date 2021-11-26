// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Contains multiple <see cref="Rect"/>-related extension methods.
    /// </summary>
	public static class RectExtensions
    {
        #region Content
        /// <inheritdoc cref="Event(Rect, out Event)"/>
        public static EventType Event(this Rect _position)
        {
            return Event(_position, out _);
        }

        /// <summary>
        /// Get the current <see cref="EventType"/> on this position.
        /// </summary>
        /// <param name="_position">Rectangle on the screen to get associated event.</param>
        /// <param name="_event">Current event.</param>
        /// <returns>Current <see cref="EventType"/> on this position (<see cref="EventType.Ignore"/> if the mouse position is not hover it).</returns>
        public static EventType Event(this Rect _position, out Event _event)
        {
            _event = UnityEngine.Event.current;
            if (_position.Contains(_event.mousePosition))
            {
                return _event.type;
            }

            return EventType.Ignore;
        }
        #endregion
    }
}
