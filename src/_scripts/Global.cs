using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class Global : Node
{
	[Export]
	private string saveFilePath;

	public string SaveFilePath => saveFilePath;

	[Export]
	private string programPath;
	public string ProgramPath => programPath;
	
	[Export]
	private string tempProgramPath;
	public string TempProgramPath => tempProgramPath;
	
	public static string programState;
	public static string checkVersionUrl = "https://github.com/yt-dlp/yt-dlp/releases";
	
	///<remarks>
	///The URL is in the format of https://github.com/yt-dlp/yt-dlp/releases/download/2024.04.09/yt-dlp.exe
	///</remarks>
	public static string downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/";
	public static string currentVersion = "";
	public static string newestVersion = "";

	public string BasePath
	{
		get {
			// var base_dir = "res://" if OS.has_feature("editor") else OS.get_executable_path().get_base_dir()
			if(OS.HasFeature("editor"))
			{
				return "res://";
			}
			else
			{
				return OS.GetExecutablePath().GetBaseDir();
			}
		}
	}

	public async Task RunCommand(string url, string arguments, Action<string> outputHandler, Action completed)
	{
		var exePath = ProjectSettings.GlobalizePath(ProgramPath);

		var startInfo = new ProcessStartInfo()
		{
			FileName = exePath,
			Arguments = arguments,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		string output = "";
		string error = "";

		using(var process = new Process() { StartInfo = startInfo})
		{
			process.OutputDataReceived += (sender, e) => 
			{
				if(string.IsNullOrEmpty(e.Data) == false)
				{
					outputHandler.Invoke(e.Data);
				}
			};

			process.Start();

			process.BeginOutputReadLine();

            await process.WaitForExitAsync();
		}

		completed.Invoke();
	}
}
