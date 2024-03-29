﻿namespace MLTrainer.Models;

public class MLScore
{
	public string Text { get; set; }
	public string IsNegative { get; set; }
	public float Importance { get; set; } = 0.5f;

	public MLScore(string text, string isNegative)
	{
		Text = text;
		IsNegative = isNegative == "0" ? "0" : "1";
	}

	public MLScore(string text, string isNegative, float importance)
	{
		Text = text;
		IsNegative = isNegative == "0" ? "0" : "1";
		Importance = importance;
	}
}
