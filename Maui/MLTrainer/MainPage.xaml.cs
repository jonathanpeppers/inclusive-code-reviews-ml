#nullable enable

using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace MLTrainer;

public partial class MainPage : ContentPage
{
	public MainPage ()
	{
		InitializeComponent ();
	}

	List<GitHubComment> GitHubComments = new List<GitHubComment> ();
	List<MLScore> Scores = new List<MLScore> ();
	int CommentCount = 0;

	async void SelectFileClicked (object sender, EventArgs e)
	{
		var file = await PickAndShow (null);

		if (file is null)
		{
			await DisplayAlert ("Issue", "File was loaded incorrectly. Try again.", "OK");
			return;
		}

		if (new FileInfo (file.FullPath).Extension != ".csv")
		{
			await DisplayAlert ("Wrong File Type", "Please select to a .csv file", "OK");
			return;
		}

		using var streamReader = new StreamReader (file.FullPath);
		using var csvReader = new CsvReader (streamReader, CultureInfo.InvariantCulture);

		GitHubComments = csvReader.GetRecords<GitHubComment> ().ToList ();

		// TODO this doesn't seem to properly check for csv files with incorrect headers
		if (!GitHubComments.Any ())
		{
			await DisplayAlert ("Wrong CSV Layout", "Please select an appropriate .csv file", "OK");
		}

		// Set up the first comment
		FileNameLabel.Text = $"FileName: {file.FullPath}";
		StartUp ();
	}

	void StartUp ()
	{
		UpdateComment();
		BadButton.IsEnabled = true;
		GoodButton.IsEnabled = true;
		SkipButton.IsEnabled = true;
		SaveButton.IsEnabled = true;
		AppendButton.IsEnabled = true;
	}

	// from https://docs.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker?tabs=windows#pick-a-file
	public async Task<FileResult?> PickAndShow (PickOptions? options)
	{
		try
		{
			var result = await FilePicker.PickAsync ();
			return result;
		}
		catch (Exception ex)
		{
			FileNameLabel.Text = $"There was an issue loading the file. Please try again.\n{ex.Message}";
		}

		return null;
	}

	bool UpdateComment ()
	{
		if (GitHubComments.Count > CommentCount)
		{
			MessageLabel.Text = GitHubComments[CommentCount].Body;
			CommentCount++;
			return true;
		}
		MessageLabel.Text = string.Empty;
		BadButton.IsEnabled = false;
		GoodButton.IsEnabled = false;
		return false;
	}

	// TODO even though we are saving the MLScore entries below surrounded by quotes,
	// the .csv file is not updated with the quotes.

	void BadCommentClicked (System.Object sender, System.EventArgs e)
	{
		Scores.Add (new MLScore ($"\"{MessageLabel.Text}\"", "\"1\""));
		UpdateComment ();
	}

	void GoodCommentClicked (System.Object sender, System.EventArgs e)
	{
		Scores.Add (new MLScore ($"\"{MessageLabel.Text}\"", "\"0\""));
		UpdateComment ();
	}

	void SkipClicked (System.Object sender, System.EventArgs e)
	{
		UpdateComment ();
	}

	// TODO figure out a better location to save the new file!
	void SaveClicked (System.Object sender, System.EventArgs e)
	{
		var csvPath = Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.csv");
		using var streamWriter = new StreamWriter (csvPath);
		using var csvWriter = new CsvWriter (streamWriter, CultureInfo.InvariantCulture);
		csvWriter.Context.RegisterClassMap<MLScoreClassMap> ();
		csvWriter.WriteRecords (Scores);

		CleanUp ();
	}

	// Append the saved answers so far to an existing csv file
	async void AppendClicked (System.Object sender, System.EventArgs e)
	{
		// We are appending so we don't need a csv header
		var config = new CsvConfiguration (CultureInfo.InvariantCulture)
		{
			HasHeaderRecord = false,
		};

		var file = await PickAndShow (null);

		if (file is null)
		{
			await DisplayAlert ("Issue", "File to save to was loaded incorrectly. Try again.", "OK");
			return;
		}

		if (new FileInfo (file.FullPath).Extension != ".csv")
		{
			await DisplayAlert ("Wrong File Type", "Please save to a .csv file", "OK");
			return;
		}

		using var stream = File.Open (file.FullPath, FileMode.Append);
		using var streamWriter = new StreamWriter (stream);
		using var csvWriter = new CsvWriter (streamWriter, config);
		csvWriter.WriteRecords (Scores);

		// TODO Validate that we are saving to a csv file with the correct headers

		await DisplayAlert("Finished!", $"Changes appended to {file.FullPath}", "OK");

		CleanUp ();
	}

	void CleanUp ()
	{
		FileNameLabel.Text = "FileName: ";
		MessageLabel.Text = string.Empty;
		BadButton.IsEnabled = false;
		GoodButton.IsEnabled = false;
		SkipButton.IsEnabled = false;
		SaveButton.IsEnabled = false;
		AppendButton.IsEnabled = false;
	}
}

// classes used for the csv formatting
public class GitHubComment
{
	[Name ("OrganizationLogin")]
	public string OrganizationLogin { get; set; } = String.Empty;

	[Name ("RepositoryName")]
	public string RepositoryName { get; set; } = String.Empty;

	[Name ("ReviewerLogin")]
	public string ReviewerLogin { get; set; } = String.Empty;

	[Name ("HtmlUrl")]
	public string HtmlUrl { get; set; } = String.Empty;

	[Name ("Body")]
	public string Body { get; set; } = String.Empty;
}

public class MLScore
{
	public string Text { get; set; }
	public string IsNegative { get; set; }

	public MLScore (string text, string isNegative)
	{
		Text = text;
		IsNegative = isNegative == "0" ? "0" : "1";
	}
}

public class MLScoreClassMap : ClassMap<MLScore>
{
	public MLScoreClassMap ()
	{
		Map (s => s.Text).Name ("text");
		Map (s => s.IsNegative).Name ("isnegative");
	}
}
