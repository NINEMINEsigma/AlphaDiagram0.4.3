
using AD.UI;
using UnityEngine;

namespace AD.Utility.Object
{
    [RequireComponent(typeof(AudioSourceController))]
    public class AudioPostMixer : MonoBehaviour
    {
        private AudioSource SelfSource = null;
        public AudioSourcePackage Package = new AudioSourcePackage();

        void Start()
        {
            SelfSource = transform.GetComponent<AudioSource>();
            SelfSource.SetSpeed(Package);
        }

        public void SetSpeed(float speed)
        {
            Package.Speed = speed;
            SelfSource.SetSpeed(Package);
        }

        public void AddSpeed(float value)
        {
            Package.Speed += value;
            SelfSource.SetSpeed(Package);
        }
    }
}