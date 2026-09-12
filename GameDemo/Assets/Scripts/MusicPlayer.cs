using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// Plays this level's music. With one candidate track it just loops; with
    /// several, one is picked at random each time the scene loads and looped
    /// for that session — so repeat visits don't always hear the same track,
    /// without needing a playlist that works through all of them in turn.
    ///
    /// Tracks are filled in automatically by the level builder from
    /// Assets/Audio/&lt;SceneName&gt;/ — drop audio files in that folder and
    /// re-run OpenDay/Build Levels.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [Tooltip("Candidate tracks for this level; one is chosen at random.")]
        [SerializeField] private AudioClip[] tracks;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.volume = SaveSystem.MusicVolume;
        }

        private void Start()
        {
            if (tracks == null || tracks.Length == 0)
            {
                return;
            }

            _source.clip = tracks[Random.Range(0, tracks.Length)];
            _source.Play();
        }
    }
}
