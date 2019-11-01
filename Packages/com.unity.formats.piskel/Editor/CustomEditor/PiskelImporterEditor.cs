using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Unity.Formats.Piskel.Editor
{
    [CustomEditor(typeof(PiskelImporter))]
    [CanEditMultipleObjects]
    public class PiskelImporterEditor : ScriptedImporterEditor
    {
    }
}
