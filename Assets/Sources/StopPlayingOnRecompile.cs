using UnityEditor;

[InitializeOnLoad]
public class StopPlayingOnRecompile
{ 
    static StopPlayingOnRecompile()
    {
        EditorApplication.update = () =>
        {
            if (EditorApplication.isCompiling && EditorApplication.isPlaying)
                EditorApplication.isPlaying = false;
        };
    }
}