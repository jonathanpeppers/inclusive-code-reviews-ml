using CsvHelper.Configuration.Attributes;

namespace MLTrainer.Models;

// classes used for the csv formatting
public class GitHubComment
{
	[Name("OrganizationLogin")]
	[Optional()]
	public string OrganizationLogin { get; set; } = string.Empty;

	[Name("RepositoryName")]
	[Optional()]
	public string RepositoryName { get; set; } = string.Empty;

	[Name("ReviewerLogin")]
	[Optional()]
	public string ReviewerLogin { get; set; } = string.Empty;

	[Name("HtmlUrl")]
	[Optional()]
	public string HtmlUrl { get; set; } = string.Empty;

	[Name("Body")]
	public string Body { get; set; } = string.Empty;
}
