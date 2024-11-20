namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Moving Average Convergence Divergence [MACD]
/// </summary>
public partial class MovingAverageConvergenceDivergence : Indicator
{
	/// <summary>
	/// Input series for the indicator.
	/// </summary>
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Fast Period"), NumericRange(1, int.MaxValue)]
	public int FastPeriod { get; set; } = 12;

	[Parameter("Slow Period"), NumericRange(1, int.MaxValue)]
	public int SlowPeriod { get; set; } = 26;

	[Parameter("Signal Period"), NumericRange(1, int.MaxValue)]
	public int SignalPeriod { get; set; } = 9;

	[Parameter("Positive Color")]
	public Color PositiveColor { get; set; } = Color.Green;

	[Parameter("Negative Color")]
	public Color NegativeColor { get; set; } = Color.Red;

	[Plot("Histogram")]
	public PlotSeries Histogram { get; set; } = new(Color.DeepPurple, PlotStyle.Histogram);

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	[Plot("Signal")]
	public PlotSeries Signal { get; set; } = new(Color.Orange);

	[Plot("Zero Line")]
	public PlotLevel ZeroLine { get; set; } = new(0, "#80787b86", LineStyle.Dash, 1);

	private MovingAverage _maLong, _maShort, _maSignal;

	public MovingAverageConvergenceDivergence()
	{
		Name = "MACD";
		ShortName = "MACD";
	}

	protected override void Initialize()
	{
		var movingAverageType = MovingAverageType.Exponential;

		_maLong = new MovingAverage(Source, SlowPeriod, movingAverageType);
		_maShort = new MovingAverage(Source, FastPeriod, movingAverageType);
		_maSignal = new MovingAverage(Result, SignalPeriod, movingAverageType);

        ShadeBetween(Result, Signal, Result.Color, Signal.Color, 0.5f);
    }

	protected override void Calculate(int index)
	{
		Result[index] = _maShort[index] - _maLong[index];
		Signal[index] = _maSignal[index];
		Histogram[index] = Result[index] - Signal[index];

		var isPositive = Histogram[index] > 0;
		var isFalling = index == 0 || Histogram[index] < Histogram[index - 1];
		var color = isPositive ? PositiveColor : NegativeColor;

		if (isPositive ? isFalling : !isFalling)
		{
			color = new((byte)Math.Round(color.A / 2.0), color.R, color.G, color.B);
		}

		Histogram.Colors[index] = color;
	}
}
