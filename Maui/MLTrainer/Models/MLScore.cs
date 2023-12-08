namespace MLTrainer.Models;

public class MLScore
{
	public string Text { get; set; }
	public string IsNegative { get; set; }

	public MLScore(string text, string isNegative)
	{
		Text = text;
		IsNegative = isNegative == "0" ? "0" : "1";
	}
}
