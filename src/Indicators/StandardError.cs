
namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Standard Error [STE]
/// </summary>
public partial class StandardError : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Orange);

	private LinearRegressionForecast _linearRegression;
	private MovingAverage _movingAverage;

	public StandardError()
	{
		Name = "Standard Error";
		ShortName = "STE";
		IsOverlay = false;
	}

	protected override void Initialize()
	{
		_linearRegression = new LinearRegressionForecast(Source, Period);
		_movingAverage = new MovingAverage(Source, Period, MovingAverageType.Simple);
	}

	protected override void Calculate(int index)
	{
		var sum = 0.0;
		var period = Math.Min(index + 1, Period);
		var averagePrice = _movingAverage[index];

		for (var i = 0; i < period; i++)
		{
			sum += Math.Pow(_linearRegression[index - i] - averagePrice, 2);
		}

		Result[index] = Math.Sqrt(sum / period) / Math.Sqrt(period);
	}
}
