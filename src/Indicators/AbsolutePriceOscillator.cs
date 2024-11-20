namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// AbsolutePriceOscillator [APO]
/// </summary>
public partial class AbsolutePriceOscillator : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Fast Period"), NumericRange(1, int.MaxValue)]
	public int FastPeriod { get; set; } = 12;

	[Parameter("Slow Period"), NumericRange(1, int.MaxValue)]
	public int SlowPeriod { get; set; } = 26;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Histogram);

	private MovingAverage _maLong, _maShort;

	public AbsolutePriceOscillator()
	{
		Name = "Absolute Price Oscillator";
		ShortName = "APO";
	}

	protected override void Initialize()
	{
		var movingAverageType = MovingAverageType.Exponential;

		_maLong = new MovingAverage(Source, SlowPeriod, movingAverageType);
		_maShort = new MovingAverage(Source, FastPeriod, movingAverageType);
	}

	protected override void Calculate(int index)
	{
		Result[index] = index < Math.Max(SlowPeriod, FastPeriod) ? 0 : _maShort[index] - _maLong[index];
		Result.Colors[index] = Result[index] > 0 ? Color.Blue : Color.Red;
	}
}
