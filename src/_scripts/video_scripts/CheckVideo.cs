using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public partial class CheckVideo : Node
{
	private Global global;

	[Export]
	private Button checkButton;

	[Export]
	private LineEdit urlInput;

	[Export]
	private Control checkingVideoPopup;

	[Export]
	private Control checkingVideoErrorPopup;

	[Export]
	private MainUIHandler mainUIHandler;

	private bool checkingVideo;
	private bool result;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		// mainUIHandler = GetNode<MainUIHandler>(mainUIHandlerPath);
		checkButton.Pressed += CheckButtonClicked;
		checkingVideo = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void CheckButtonClicked()
	{
		checkingVideoPopup.Visible = true;
		GetVideoTitle();
	}

	private void GetVideoTitle()
	{
		var videoTitle = "";
		var url = urlInput.Text;
		var arguments = $"--get-title --skip-download {url}";

		result = true;
		var firstCheck = true;
		void outputHandler(string output)
		{
			if(firstCheck)
			{
				if(output.StartsWith("ERROR"))
				{
					result = false;
				}
				firstCheck = false;
			}
			GD.Print(output);

			videoTitle = output;	
		}

		void completedHandler()
		{
			if(result)
			{
				mainUIHandler.Title.Text = videoTitle;
				GD.Print("Happens?");
				GetVideoRes();
			}
			else
			{
				checkingVideoPopup.Visible = false;
				checkingVideoErrorPopup.Visible = true;
			}
		}

		_ = global.RunCommand(url, arguments, outputHandler, completedHandler);
	}

	private void GetVideoRes()
	{
		var url = urlInput.Text;
		var arguments = $"--list-formats {url}";

		var resolutionList = new HashSet<string>();

        void outputHandler(string output)
        {
            var separated = output.Split(" ").Where(stri => stri != "").ToArray();
            var mp4Idx = Array.IndexOf(separated, "mp4");
            if (mp4Idx == -1)
            {
                return;
            }

            var typeIdx = mp4Idx + 1;
            if (separated[typeIdx] == "audio")
            {
                return;
            }

            resolutionList.Add(separated[typeIdx]);
        }

        void completedHandler()
        {
            // checkingVideoPopup.Visible = false;
            mainUIHandler.AddResolutionButtons(resolutionList.ToArray());
			GetVideoThumbnail();
        }

        _ = global.RunCommand(url, arguments, outputHandler, completedHandler);
	}

	private void GetVideoThumbnail()
	{
		var imageDownloadPath = "user://_downloads/thumbnail";
		var imageDownloadPathGlobalized = ProjectSettings.GlobalizePath(imageDownloadPath);
		var url = urlInput.Text;
		var arguments = $" --write-thumbnail --skip-download --convert-thumbnails jpg -o {imageDownloadPathGlobalized} {url}";

		void outputHandler(string output)
		{
		}

		void completedHandler()
		{
			// mainUIHandler.Thumbnail.Texture = GD.Load<Texture2D>(imageDownloadPath + ".jpg");
			var image = Image.LoadFromFile(imageDownloadPath + ".jpg");
			var imageTexture = ImageTexture.CreateFromImage(image);
			mainUIHandler.Thumbnail.Texture = imageTexture;
			checkingVideoPopup.Visible = false;
		}

		_ = global.RunCommand(url, arguments, outputHandler, completedHandler);
	}
}
