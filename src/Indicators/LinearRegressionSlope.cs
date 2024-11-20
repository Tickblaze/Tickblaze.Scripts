namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Linear Regression Slope [LRS]
/// </summary>
public partial class LinearRegressionSlope : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 9;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Green);

	public LinearRegressionSlope()
	{
		Name = "Linear Regression Slope";
		ShortName = "LRS";
	}

	protected override void Calculate(int index)
	{
		if (index <= Period)
		{
			return;
		}

		var sumX = 0.0;
		var sumY = 0.0;
		var sumX2 = 0.0;
		var sumXY = 0.0;

		for (var i = 0; i < Period; i++)
		{
			var barIndex = (double)(i + 1);
			var value = Source[index - Period + i + 1];

			sumX += barIndex;
			sumY += value;
			sumX2 += barIndex * barIndex;
			sumXY += barIndex * value;
		}

		Result[index] = (sumXY * Period - sumX * sumY) / (sumX2 * Period - Math.Pow(sumX, 2.0));
	}
}
