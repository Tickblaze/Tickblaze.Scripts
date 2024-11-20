namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Linear Regression Intercept [LRI]
/// </summary>
public partial class LinearRegressionIntercept : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 9;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Orange);

	private LinearRegressionSlope _slope;
	private SimpleMovingAverage _sma;

	public LinearRegressionIntercept()
	{
		Name = "Linear Regression Intercept";
		ShortName = "LRI";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_slope = new LinearRegressionSlope(Source, Period);
		_sma = new SimpleMovingAverage(Source, Period);
	}

	protected override void Calculate(int index)
	{
		Result[index] = _sma[index] - _slope[index] * Math.Floor(Period / 2.0);
	}
}
