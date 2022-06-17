#pragma warning disable CA1416 // Not available on Adroid 21 and later

using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace MLTrainer;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	List<GitHubComment> GitHubComments;
	List<MLScore> Scores = new List<MLScore> ();
	int CommentCount = 0;

	async void SelectFileClicked (object sender, EventArgs e)
	{
		var file = await PickAndShow (null);
		//var file = await PickAndShow (CsvOption);


		using var streamReader = new StreamReader (file.FullPath);
		using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
		GitHubComments = csvReader.GetRecords<GitHubComment> ().ToList ();


		BadButton.IsEnabled = true;
		GoodButton.IsEnabled = true;
		FileNameLabel.Text = $"FileName: {file.FullPath}";
		UpdateComment ();
	}

	// from https://docs.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker?tabs=windows#pick-a-file
	public async Task<FileResult> PickAndShow (PickOptions options)
	{
		try
		{
			var result = await FilePicker.PickAsync ();
			return result;
		}
		catch (Exception ex)
		{
			// The user canceled or something went wrong
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
		BadButton.IsEnabled = false;
		GoodButton.IsEnabled = false;
		return false;
	}

	void BadCommentClicked(System.Object sender, System.EventArgs e)
	{
		Scores.Add (new MLScore ($"\"{MessageLabel.Text}\"", "\"1\""));
		UpdateComment ();
	}

	void GoodCommentClicked(System.Object sender, System.EventArgs e)
	{
		Scores.Add(new MLScore($"\"{MessageLabel.Text}\"", "\"0\""));
		UpdateComment ();
	}

	void SkipClicked(System.Object sender, System.EventArgs e)
	{
		UpdateComment ();
	}

	void SaveClicked(System.Object sender, System.EventArgs e)
	{
		var csvPath = Path.Combine (Environment.CurrentDirectory, "temp.csv");
		using var streamWriter = new StreamWriter (csvPath);
		using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
		csvWriter.Context.RegisterClassMap<MLScoreClassMap> ();
		csvWriter.WriteRecords (Scores);
	}

	async void AppendClicked(System.Object sender, System.EventArgs e)
	{
		// Append to the file.
		var config = new CsvConfiguration (CultureInfo.InvariantCulture)
		{
			// Don't write the header again.
			HasHeaderRecord = false,
		};

		var file = await PickAndShow (null);

		using var stream = File.Open (file.FullPath, FileMode.Append);
		using var streamWriter = new StreamWriter (stream);
		using var csvWriter = new CsvWriter(streamWriter, config);
		csvWriter.WriteRecords (Scores);
	}


}

public class GitHubComment
{
	[Name ("OrganizationLogin")]
	public string OrganizationLogin { get; set; }

	[Name("RepositoryName")]
	public string RepositoryName { get; set; }

	[Name("ReviewerLogin")]
	public string ReviewerLogin { get; set; }

	[Name("HtmlUrl")]
	public string HtmlUrl { get; set; }

	[Name("Body")]
	public string Body { get; set; }
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
