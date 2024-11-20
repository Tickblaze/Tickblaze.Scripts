
namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Time Series Forecast [TSF]
/// </summary>
public partial class TimeSeriesForecast : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Forecast Period"), NumericRange(1, int.MaxValue)]
	public int Forecast { get; set; } = 3;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	private LinearRegressionSlope _slope;
	private LinearRegressionIntercept _intercept;

	public TimeSeriesForecast()
	{
		Name = "Time Series Forecast";
		ShortName = "TSF";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_slope = new(Source, Period);
		_intercept = new(Source, Period);
	}

	protected override void Calculate(int index)
	{
		Result[index] = (Period + Forecast - 1) * _slope[index] + _intercept[index];
	}
}
