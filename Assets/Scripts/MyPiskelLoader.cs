using System.IO;
using Unity.Formats.Piskel;
using UnityEngine;

public class MyPiskelLoader : MonoBehaviour
{
    public string piskelPath;

    void Start()
    {
        var piskel = File.ReadAllText(piskelPath);
        var piskelRoot = PiskelRuntimePlayer.CreateFromPiskelFile(piskel, new PiskelSpriteSettings());
        piskelRoot.transform.parent = transform;
    }
}
