using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// Plays a level's music. One track loops on its own; several are played
    /// as a playlist, advancing when each finishes and wrapping at the end.
    /// Each level owns its music, so the track changing with the scene is the
    /// intended behaviour rather than something to work around.
    ///
    /// Tracks are filled in automatically by the level builder from
    /// Assets/Audio/&lt;SceneName&gt;/ — drop audio files in that folder and
    /// re-run OpenDay/Build Levels.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [Tooltip("Tracks for this level, played in order.")]
        [SerializeField] private AudioClip[] tracks;

        [Tooltip("Play the tracks in a random order instead of the order listed.")]
        [SerializeField] private bool shuffle;

        private AudioSource _source;
        private int _index;

        private bool HasTracks => tracks != null && tracks.Length > 0;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.volume = SaveSystem.MusicVolume;
        }

        private void Start()
        {
            if (!HasTracks)
            {
                return;
            }

            if (shuffle)
            {
                Shuffle();
            }

            PlayCurrent();
        }

        private void Update()
        {
            // A finished track means it's time for the next one. A lone track
            // loops on the AudioSource itself, so this never fires for it.
            if (!HasTracks || _source.isPlaying)
            {
                return;
            }

            _index = (_index + 1) % tracks.Length;
            PlayCurrent();
        }

        private void PlayCurrent()
        {
            _source.clip = tracks[_index];
            _source.loop = tracks.Length == 1;
            _source.Play();
        }

        private void Shuffle()
        {
            for (var i = tracks.Length - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (tracks[i], tracks[j]) = (tracks[j], tracks[i]);
            }
        }
    }
}
