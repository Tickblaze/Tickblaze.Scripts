namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Linear Regression RSquared [LRR]
/// </summary>
public partial class LinearRegressionRSquared : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 9;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Green);

	public LinearRegressionRSquared()
	{
		Name = "Linear Regression RSquared";
		ShortName = "LRR";
	}

	protected override void Calculate(int index)
	{
		var period = Math.Min(Period, index + 1);

		var sumX = 0;
		var sumY = 0.0;
		var sumX2 = 0;
		var sumY2 = 0.0;
		var sumXY = 0.0;

		for (var i = 0; i < period; i++)
		{
			var idx = i + 1;
			var source = Source[index - period + i + 1];

			sumX += idx;
			sumY += source;
			sumX2 += idx * idx;
			sumY2 += source * source;
			sumXY += idx * source;
		}

		var numerator = period * sumXY - sumX * sumY;
		var denominator = Math.Sqrt((period * sumX2 - sumX * sumX) * (period * sumY2 - sumY * sumY));
		var rSquared = Math.Pow(numerator / denominator, 2.0);

		Result[index] = rSquared;
	}
}
