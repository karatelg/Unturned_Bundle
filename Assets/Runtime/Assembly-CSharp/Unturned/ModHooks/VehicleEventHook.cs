using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to Vehicle GameObject to receive events.
	/// </summary>
	[AddComponentMenu("Unturned/Vehicle Event Hook")]
	public class VehicleEventHook : MonoBehaviour
	{
		/// <summary>
		/// Invoked when any player enters the driver seat.
		/// </summary>
		public UnityEvent OnDriverAdded;
		
		/// <summary>
		/// Invoked when any player exits the driver seat.
		/// </summary>
		public UnityEvent OnDriverRemoved;

		/// <summary>
		/// Invoked when a locally controlled player enters the driver seat.
		/// </summary>
		public UnityEvent OnLocalDriverAdded;

		/// <summary>
		/// Invoked when a locally controlled player exits the driver seat.
		/// </summary>
		public UnityEvent OnLocalDriverRemoved;

		/// <summary>
		/// Invoked when lock is engaged.
		/// </summary>
		public UnityEvent OnLocked;

		/// <summary>
		/// Invoked when lock is disengaged.
		/// </summary>
		public UnityEvent OnUnlocked;
	}
}
