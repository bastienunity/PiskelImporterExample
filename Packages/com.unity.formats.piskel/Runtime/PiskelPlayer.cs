using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.Formats.Piskel
{
    [RequireComponent(typeof(Animator))]
    public class PiskelPlayer : MonoBehaviour, IAnimationClipSource
    {
        public AnimationClip clip;
        PlayableGraph playableGraph;

        void Start()
        {
            AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), clip, out playableGraph);
        }

        void OnDestroy()
        {
            // Destroys all Playables and Outputs created by the graph.
            if (playableGraph.IsValid())
                playableGraph.Destroy();
        }

        public void GetAnimationClips(List<AnimationClip> results)
        {
            results.Add(clip);
        }
    }
}
