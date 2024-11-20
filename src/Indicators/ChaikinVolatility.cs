namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Chaikin Volatility [CV]
/// </summary>
public partial class ChaikinVolatility : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Rate of Change"), NumericRange(1, int.MaxValue)]
	public int RateOfChange { get; set; } = 10;

	[Parameter("Smoothing Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Exponential;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	private DataSeries _rangeSeries;
	private MovingAverage _movingAverage;

	public ChaikinVolatility()
	{
		Name = "Chaikin Volatility";
		ShortName = "CV";
	}

	protected override void Initialize()
	{
		_rangeSeries = new DataSeries();
		_movingAverage = new MovingAverage(_rangeSeries, Period, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		_rangeSeries[index] = Bars.High[index] - Bars.Low[index];

		var pastIndex = Math.Max(0, index - RateOfChange);
		var pastMovingAverage = _movingAverage.Result[pastIndex];
		var currentMovingAverage = _movingAverage.Result[index];

		Result[index] = (currentMovingAverage - pastMovingAverage) / pastMovingAverage * 100.0;
	}
}
