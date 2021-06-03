using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SDG.Unturned
{
	[AddComponentMenu("Unturned/Particle System Collision Audio")]
	public class ParticleSystemCollisionAudio : MonoBehaviour
	{
		public ParticleSystem particleSystemComponent;

		[FormerlySerializedAs("audioPrefab")]
		public OneShotAudioDefinition audioDef;

		/// <summary>
		/// Collision with speed lower than this value will not play a sound.
		/// </summary>
		public float speedThreshold = 0.01f;
		
		public float minSpeed = 0.2f;
		public float maxSpeed = 1.0f;
		public float minVolume = 0.5f;
		public float maxVolume = 1.0f;

#if !DEDICATED_SERVER
		private void OnParticleCollision(GameObject other)
		{
			if (particleSystemComponent == null || other == null || audioDef == null)
				return;

			float speedThresholdSquared = speedThreshold * speedThreshold; 

			int eventCount = particleSystemComponent.GetCollisionEvents(other, collisionEvents);
			for(int eventIndex = 0; eventIndex < eventCount; ++eventIndex)
			{
				ParticleCollisionEvent item = collisionEvents[eventIndex];
				
				float speedSquared = item.velocity.sqrMagnitude;
				if (speedSquared < speedThresholdSquared)
					continue;

				float speed = Mathf.Sqrt(speedSquared);
				float normalizedVolume = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
				float volume = Mathf.Lerp(minVolume, maxVolume, normalizedVolume);

#if GAME
				AudioClip clip = audioDef.GetRandomClip();
				OneShotAudioParameters oneShotParams = new OneShotAudioParameters(item.intersection, clip);
				oneShotParams.volume = volume * audioDef.volumeMultiplier;
				oneShotParams.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);
				oneShotParams.Play();
#endif // GAME
			}
		}

		[System.NonSerialized]
		private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
#endif // !DEDICATED_SERVER
	}
}
