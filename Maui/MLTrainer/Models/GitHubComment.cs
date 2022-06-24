#nullable enable

using CsvHelper.Configuration.Attributes;

namespace MLTrainer.Models;

// classes used for the csv formatting
public class GitHubComment
{
    [Name("OrganizationLogin")]
    public string OrganizationLogin { get; set; } = string.Empty;

    [Name("RepositoryName")]
    public string RepositoryName { get; set; } = string.Empty;

    [Name("ReviewerLogin")]
    public string ReviewerLogin { get; set; } = string.Empty;

    [Name("HtmlUrl")]
    public string HtmlUrl { get; set; } = string.Empty;

    [Name("Body")]
    public string Body { get; set; } = string.Empty;
}
