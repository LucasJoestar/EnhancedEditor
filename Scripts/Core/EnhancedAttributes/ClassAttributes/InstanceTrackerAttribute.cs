// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// Creates an InstanceTracker <see cref="ScriptableObject"/> associated with this class object type (must be either a <see cref="Component"/> or an Interface),
    /// registering all of its instances in each scene of the project.
    /// <para/>
    /// Allowing users to ping these instances in their scene from a simple button,
    /// <br/> these trackers facilitates important scene objects management in the project.
    /// </summary>
    public class InstanceTrackerAttribute : EnhancedClassAttribute { }
}
