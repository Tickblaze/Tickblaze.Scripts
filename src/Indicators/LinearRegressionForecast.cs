namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Linear Regression Forecast [LRF]
/// </summary>
public partial class LinearRegressionForecast : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 9;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Orange);

	private LinearRegressionSlope _slope;
	private LinearRegressionIntercept _intercept;

	public LinearRegressionForecast()
	{
		Name = "Linear Regression Forecast";
		ShortName = "LRF";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_slope = new(Source, Period);
		_intercept = new(Source, Period);
	}

	protected override void Calculate(int index)
	{
		Result[index] = Period * _slope[index] + _intercept[index];
	}
}
