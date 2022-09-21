using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace MLTrainer.Models;

// classes used for the csv formatting
public class GitHubCommentClassMap : ClassMap<GitHubComment>
{
	public GitHubCommentClassMap()
	{
		Map(s => s.Body).Name("Body");
	}
}
