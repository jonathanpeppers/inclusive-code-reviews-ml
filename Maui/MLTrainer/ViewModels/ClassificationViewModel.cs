using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using MLTrainer.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MLTrainer.ViewModels;

public partial class ClassificationViewModel : ObservableObject
{
    readonly List<MLScore> _scores = new();
    int _commentCount;

    [ObservableProperty]
    string fileName = String.Empty;

    [ObservableProperty]
    string message = String.Empty;

    [ObservableProperty]
    string status = String.Empty;

    [ObservableProperty]
    ObservableCollection<GitHubComment>? gitHubComments;

    Task DisplayAlert (string title, string message, string cancel)
    {
        return App.Current!.MainPage!.DisplayAlert(title, message, cancel);
    }

    [RelayCommand]
    async Task SelectedFile()
    {
        var file = await PickAndShow(null);

        if (file is null)
        {
            await DisplayAlert("Issue", "File was loaded incorrectly. Try again.", "OK");
            return;
        }

        if (new FileInfo(file.FullPath).Extension != ".csv")
        {
            await DisplayAlert("Wrong File Type", "Please select to a .csv file", "OK");
            return;
        }

        using var streamReader = new StreamReader(file.FullPath);
        using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

        GitHubComments = new ObservableCollection<GitHubComment>(csvReader.GetRecords<GitHubComment>());

        // TODO this doesn't seem to properly check for csv files with incorrect headers
        if (!GitHubComments.Any())
        {
            await DisplayAlert("Wrong CSV Layout", "Please select an appropriate .csv file", "OK");
        }

        FileName = file.FileName;

        UpdateComment();
        UpdateButtons();
    }

    [RelayCommand(CanExecute = "DoWeHaveComments")]
    void GoodComment()
    {
        _scores.Add(new MLScore($"\"{Message}\"", "\"0\""));
        UpdateComment();
    }

    [RelayCommand(CanExecute = "DoWeHaveComments")]
    void BadComment()
    {
        _scores.Add(new MLScore($"\"{Message}\"", "\"1\""));
        UpdateComment();
    }

    [RelayCommand(CanExecute = "DoWeHaveComments")]
    void SkipComment()
    {
        UpdateComment();
    }

    [RelayCommand(CanExecute = "DoWeHaveScores")]
    void SaveFile()
    {
        var csvPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.csv");
        using var streamWriter = new StreamWriter(csvPath);
        using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        csvWriter.Context.RegisterClassMap<MLScoreClassMap>();
        csvWriter.WriteRecords(_scores);
        CleanUp();
    }

    [RelayCommand(CanExecute = "DoWeHaveScores")]
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

        // TODO Validate that we are saving to a csv file with the correct headers
        await DisplayAlert("Finished!", $"Changes appended to {file.FullPath}", "OK");
    }

    bool DoWeHaveComments() => (GitHubComments != null) && GitHubComments.Count > _commentCount;

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
            FileName = $"There was an issue loading the file. Please try again.\n{ex.Message}";
        }

        return null;
    }

    bool UpdateComment()
    {
        if (GitHubComments?.Count > _commentCount)
        {
            Message = GitHubComments[_commentCount].Body;
            _commentCount++;
            Status = $"{_commentCount} / {GitHubComments.Count} done ({100.0 * _commentCount / GitHubComments.Count:0.00} %)";
            UpdateFileButtons();
            return true;
        }
        Message = string.Empty;
        Status = "100% done";
        UpdateButtons();
        return false;
    }
}
