namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Stochastic Relative Strength Index [StochRSI]
/// </summary>
public sealed partial class StochasticRelativeStrengthIndex : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Overbought")]
	public PlotLevel OverboughtLevel { get; set; } = new(0.8, Color.Red);

	[Plot("Middle")]
	public PlotLevel MiddleLevel { get; set; } = new(0.5, Color.SteelGray);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(0.2, Color.Green);

	private RelativeStrengthIndex _rsi;
	private Minimum _min;
	private Maximum _max;

	public StochasticRelativeStrengthIndex()
	{
		Name = "Stochastic RSI";
		ShortName = "StochRSI";
	}

	protected override void Initialize()
	{
		_rsi = new RelativeStrengthIndex(Source, Period, MovingAverageType.Simple, 1);
		_min = new Minimum(_rsi.Result, Period);
		_max = new Maximum(_rsi.Result, Period);
	}
	protected override void Calculate(int index)
	{
		var rsi = _rsi.Result[index];
		var minimum = _min.Result[index];
		var maximum = _max.Result[index];

		Result[index] = rsi != minimum && maximum != minimum ? (rsi - minimum) / (maximum - minimum) : 0;
	}
}