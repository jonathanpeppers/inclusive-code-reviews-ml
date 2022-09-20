using System;
using System.Text.RegularExpressions;

namespace MLTrainer
{
	public static class TextProcessor
	{
		const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
		static readonly Regex _githubHandleRegex = new Regex(@"\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))", Options);
		static readonly Regex _backtickRegex = new Regex("`+[^`]+`+", Options);
		static readonly Regex _punctuationRegex = new Regex("(\\.|!|\\?|;|:)+$", Options);

		static string ReplaceGithubHandles(string text) => _githubHandleRegex.Replace(text, "@github");

		static string ReplaceInlineBackticks(string text) => _backtickRegex.Replace(text, "#code");

		static string ReplaceTrailingPunctuation(string text) => _punctuationRegex.Replace(text, "");

		static string RemoveImageTags(string text)
		{
			var startTag = text.IndexOf("![", StringComparison.Ordinal);
			var result = text;
			while (startTag >= 0)
			{
				var endTag = result.IndexOf("](", startTag + 2, StringComparison.Ordinal);
				if (endTag == -1)
					break;
				var end = result.IndexOf(')', endTag + 2);
				if (end == -1)
					break;
				var alttext = result.Substring(startTag + 2, endTag - startTag + 2);
				var space = new string(' ', end - endTag);
				result = result.Substring(0, startTag) + ". " + alttext + "." + space + result.Substring(end + 1);
				startTag = result.IndexOf("![", end + 1, StringComparison.Ordinal);
			}
			return result;
		}

		// Replace fenced / indented code blocks with spaces to not analyze them
		static string RemoveCodeBlocks(string text)
		{
			var atStartOfLine = true;
			var inFencedCodeBlock = false;
			var inIndentedCodeBlock = false;
			var result = "";
			for (var i = 0; i < text.Length; i++)
			{
				var c = text[i];
				if (c == '\n')
				{
					atStartOfLine = true;
					inIndentedCodeBlock = false;
					result += c;
					continue;
				}

				if (!atStartOfLine)
				{
					if (inIndentedCodeBlock || inFencedCodeBlock)
					{
						result += " ";
					}
					else
					{
						result += c;
					}
					continue;
				}

				if (atStartOfLine && c == '`' && text.Length >= i + 3 && text.Substring(i, 3) == "```")
				{
					inFencedCodeBlock = !inFencedCodeBlock;
					result += "   ";
					i += 2;
					atStartOfLine = false;
					continue;
				}

				if (inFencedCodeBlock)
				{
					result += " ";
					atStartOfLine = false;
					continue;
				}

				if (atStartOfLine && c == ' ' && text.Length >= i + 4 && text.Substring(i, 4) == "    ")
				{
					inIndentedCodeBlock = true;
					result += "    ";
					i += 3;
					atStartOfLine = false;
					continue;
				}

				result += c;
				atStartOfLine = false;
			}

			return result;
		}

		public static string[] PreprocessText(string text)
		{
			var result = RemoveCodeBlocks(text);
			result = RemoveImageTags(result);

			var extraTrimCharacters = new char[] { ' ', '\t', '\n', '\r', '>' };
			var sentences = result.Split(new char[] { '\n', '\r' })
								.Where(v => v.Length > 0)
								.Select(v =>
								{
									var startIndex = 0;
									while (startIndex < v.Length)
									{
										var ch = v[startIndex];
										if (char.IsWhiteSpace(ch))
											startIndex++;
										else if (ch == '>')
											startIndex++;
										else
											break;
									}
									if (startIndex >= v.Length)
										return string.Empty;
									var endIndex = v.Length - 1;
									while (endIndex >= 0)
									{
										var ch = v[endIndex];
										if (char.IsWhiteSpace(ch))
											endIndex--;
										else
											break;

									}
									if (endIndex <= 0)
										return string.Empty;
									if (startIndex == 0 && endIndex == v.Length - 1)
										return v;
									return v.Substring(startIndex, endIndex - startIndex + 1);
								})
								.Where(v => v.Length > 0)
								.Select(ReplaceGithubHandles)
								.Select(ReplaceInlineBackticks)
								.Select(ReplaceTrailingPunctuation)
								.Distinct()
								.ToArray();

			return sentences;
		}
	}
}
