using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using MLTrainer.Models;

namespace MLTrainer.ViewModels;

public partial class ClassificationViewModel : ObservableObject
{
	readonly List<MLScore> _scores = new();
	int _sentenceCount;
	string _fullPath = string.Empty;
	FileResult? _initialFile;
	Dictionary<GitHubComment, List<Sentence>> _comments = new();

	[ObservableProperty]
	string _fileName = String.Empty;

	[ObservableProperty]
	string _message = String.Empty;

	[ObservableProperty]
	string _status = String.Empty;

	[ObservableProperty]
	Color _goodColor = Color.FromHsla(0.314, 1.0, 0.25, 1.0);

	[ObservableProperty]
	Color _badColor = Color.FromHsla(0, 1.0, 0.35, 1.0);

	[ObservableProperty]
	float _goodValue = 0.5f;

	[ObservableProperty]
	float _badValue = 0.5f;

	[ObservableProperty]
	string _goodText = "Good Comment (F2). Rating: 0.5";

	[ObservableProperty]
	string _badText = "Bad Comment(F1). Rating: 0.5";

	[ObservableProperty]
	ObservableCollection<GitHubComment>? _gitHubComments;

	[ObservableProperty]
	ObservableCollection<Sentence> _sentences = new();


	[RelayCommand]
	async Task SelectedFile()
	{
		_initialFile = await PickAndShow(null);

		if (_initialFile is null)
		{
			await DisplayAlert("Issue", "File was loaded incorrectly. Try again.", "OK");
			return;
		}

		if (new FileInfo(_initialFile.FullPath).Extension != ".csv")
		{
			await DisplayAlert("Wrong File Type", "Please select to a .csv file", "OK");
			return;
		}

		using var streamReader = new StreamReader(_initialFile.FullPath);
		using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

		try
		{
			var getrecords = csvReader.GetRecords<GitHubComment>();
			GitHubComments = new ObservableCollection<GitHubComment>(getrecords);
		}
		catch (HeaderValidationException v)
		{
			await DisplayAlert("Wrong File configuration", $"Please make sure there is a Header column with 'Body' text. {v.Message}", "OK");
			return;
		}
	
		// TODO this doesn't seem to properly check for csv files with incorrect headers
		if (!GitHubComments.Any())
		{
			await DisplayAlert("Wrong CSV Layout", "Please select an appropriate .csv file", "OK");
		}

		foreach (var comment in GitHubComments)
		{
			var githubSentences = new List<Sentence>();
			var processedSentences = TextProcessor.PreprocessText(comment.Body);
			foreach (var processedSentence in processedSentences)
			{
				var sentence = new Sentence { Body = processedSentence };
				_sentences.Add(sentence);
				githubSentences.Add(sentence);
			}
			_comments.Add(comment, githubSentences);
		}

		FileName = _initialFile.FileName;
		_fullPath = _initialFile.FullPath;

		UpdateComment();
		UpdateButtons();
	}

	[RelayCommand(CanExecute = nameof(DoWeHaveComments))]
	void GoodComment()
	{
		_scores.Add(new MLScore(Message, "0", (float)Math.Round (GoodValue, 1)));
		UpdateComment();
	}

	[RelayCommand(CanExecute = nameof(DoWeHaveComments))]
	void BadComment()
	{
		_scores.Add(new MLScore(Message, "1", (float)Math.Round (BadValue, 1)));
		UpdateComment();
	}

	[RelayCommand(CanExecute = nameof(DoWeHaveComments))]
	void SkipComment()
	{
		UpdateComment();
	}

	[RelayCommand(CanExecute = nameof(DoWeHaveScores))]
	async void SaveFile()
	{
		try
		{
			var csvPath = Path.Combine(Path.GetDirectoryName(_fullPath)!, Path.GetFileNameWithoutExtension(_fullPath) + "-saved.csv");
			using var streamWriter = new StreamWriter(csvPath);
			using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
			csvWriter.Context.RegisterClassMap<MLScoreClassMap>();
			csvWriter.WriteRecords(_scores);
			streamWriter.Write(Environment.NewLine); // Trailing newline
			CleanUp();
			await DisplayAlert("Success!", $"Saved to: {csvPath}", "OK");

			await RemoveLinesFromInitialFile();
		}
		catch (Exception e)
		{
			await DisplayAlert("Problem!", $"Failed to save the file: {e.Message}", "OK");
		}
	}

	[RelayCommand(CanExecute = nameof(DoWeHaveScores))]
	async Task AppendFile()
	{
		// We are appending so we don't need a csv header
		var config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HasHeaderRecord = false,
		};

		var file = await PickAndShow(null);

		if (file is null)
		{
			await DisplayAlert("Issue", "File to save to was loaded incorrectly. Try again.", "OK");
			return;
		}

		if (new FileInfo(file.FullPath).Extension != ".csv")
		{
			await DisplayAlert("Wrong File Type", "Please save to a .csv file", "OK");
			return;
		}

		using var stream = File.Open(file.FullPath, FileMode.Append);
		using var streamWriter = new StreamWriter(stream);
		using var csvWriter = new CsvWriter(streamWriter, config);
		csvWriter.WriteRecords(_scores);
		streamWriter.Write(Environment.NewLine); // Trailing newline

		// TODO Validate that we are saving to a csv file with the correct headers
		await DisplayAlert("Finished!", $"Changes appended to {file.FullPath}", "OK");

		await RemoveLinesFromInitialFile();
	}

	[RelayCommand (CanExecute = nameof (DoWeHaveComments))]
	void GoodSliderChange ()
	{
		UpdateGoodSliderElements (null);
	}

	void UpdateGoodSliderElements (float? value)
	{
		if (value is float v)
			GoodValue = v;
		GoodText = $"Good Comment (F2). Rating {string.Format("{0:0.0}", GoodValue)}";
		GoodColor = Color.FromHsla(0.314, 1.0, 0.2 + GoodValue* 0.1, 1.0);
	}

	[RelayCommand (CanExecute = nameof (DoWeHaveComments))]
	void BadSliderChange ()
	{
		UpdateBadSliderElements (null);
	}

	void UpdateBadSliderElements (float? value)
	{
		if (value is float v)
			BadValue = v;
		BadText = $"Bad Comment (F1). Rating {string.Format("{0:0.0}", BadValue)}";
		BadColor = Color.FromHsla (0, 1.0, 0.3 + BadValue * 0.1, 1.0);
	}

	bool DoWeHaveComments() => (Sentences != null) && Sentences.Count > _sentenceCount;

	bool DoWeHaveScores() => _scores.Count > 0;

	void UpdateButtons()
	{
		UpdateCommentButtons();
		UpdateFileButtons();
	}

	void UpdateCommentButtons()
	{
		GoodCommentCommand.NotifyCanExecuteChanged();
		BadCommentCommand.NotifyCanExecuteChanged();
		SkipCommentCommand.NotifyCanExecuteChanged();
	}

	void UpdateFileButtons()
	{
		SaveFileCommand.NotifyCanExecuteChanged();
		AppendFileCommand.NotifyCanExecuteChanged();
	}

	void CleanUp()
	{
		FileName = string.Empty;
		Message = string.Empty;
		Status = string.Empty;
		UpdateButtons();
		//Should we clean _scores?
	}

	async Task<FileResult?> PickAndShow(PickOptions? options)
	{
		try
		{
			var result = await FilePicker.PickAsync();
			return result;
		}
		catch (Exception ex)
		{
			FileName = $"There was an issue loading the file. Please try again.\n{ex.ToString()}";
		}

		return null;
	}

	bool UpdateComment()
	{
		UpdateGoodSliderElements (0.5f);
		UpdateBadSliderElements (0.5f);

		if (Sentences?.Count > _sentenceCount)
		{
			//Mark as processed the previous sentence
			if (_sentenceCount > 0)
				Sentences[_sentenceCount - 1].Processed = true;
			Message = Sentences[_sentenceCount].Body;
			_sentenceCount++;
			Status = $"{_sentenceCount}/{Sentences.Count} done ({100.0 * _sentenceCount / Sentences.Count:0.00} %)";
			UpdateFileButtons();
			return true;
		}
		Message = string.Empty;
		Status = "100% done";
		UpdateButtons();
		return false;
	}

	async Task RemoveLinesFromInitialFile()
	{
		var result = await DisplayAlert("Remove lines", "Do you want to remove classified comments from the initial load file?", "Yes", "No");
		if (result && _initialFile != null)
		{
			var commentsToRemove = _comments.Where(c => c.Value.Any(c => c.Processed)).ToList();

			foreach (var item in commentsToRemove)
			{
				GitHubComments?.Remove(item.Key);
			}

			using var streamWriter = new StreamWriter(_initialFile.FullPath);
			using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
			csvWriter.Context.RegisterClassMap<GitHubCommentClassMap>();
			csvWriter.WriteRecords(GitHubComments);

			await DisplayAlert("Success!", $"Removed {commentsToRemove.Count} comments from: {_initialFile.FileName}", "OK");
		}
	}

	static Task DisplayAlert(string title, string message, string cancel) =>
		App.Current!.MainPage!.DisplayAlert(title, message, cancel);


	static Task<bool> DisplayAlert(string title, string message, string ok, string cancel) =>
		App.Current!.MainPage!.DisplayAlert(title, message, ok, cancel);

}
