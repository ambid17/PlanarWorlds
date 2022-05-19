using UnityEditor;

[CustomEditor(typeof(README))]
public class READMEEditor : Editor
{
    private const string Text = "To start digging in the terrain, go to the menu 'Tools > Digger' and click on 'Setup terrains'.\n\n" +
                                "Then, select the 'Digger Master' object in the scene and click on the terrain to dig.\n\n" +
                                "You can change brush, opacity and texture from the Digger Master inspector.\n\n" +
                                "Only with Digger Runtime module (or PRO edition): you can also enable realtime/in-game editing by clicking on 'Tools > Digger > Setup for runtime' menu.\n\n" +
                                "Only with Digger Runtime module (or PRO edition): you can also enable in-game NavMesh updating by clicking on 'Tools > Digger > Setup NavMeshComponents' menu.\n\n" +
                                "DOCUMENTATION: https://ofux.github.io/Digger-Documentation/";

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(Text, MessageType.Info);
    }
}