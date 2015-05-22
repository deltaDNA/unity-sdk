using UnityEditor;

public class BuildScript {

	static void BuildAndroidEclipseProject()
	{
		string[] scenes = { "Assets/DeltaDNA/Example/deltaDNA.unity" };
		string outputPath = "Build/Android";
		BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.Development);
	}
}
