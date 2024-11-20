namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Triangular Moving Average [TMA]
/// </summary>
public partial class TriangularMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid);

	private SimpleMovingAverage _sma1, _sma2;

	public TriangularMovingAverage()
	{
		Name = "Triangular Moving Average";
		ShortName = "TMA";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		int p1, p2;

		if ((Period % 1) == 0)
		{
			p1 = Period / 2;
			p2 = p1 + 1;
		}
		else
		{
			p1 = (Period + 1) / 2;
			p2 = p1;
		}

		_sma1 = new SimpleMovingAverage(Source, p1);
		_sma2 = new SimpleMovingAverage(_sma1.Result, p2);
	}

	protected override void Calculate(int index)
	{
		Result[index] = _sma2[index];
	}
}
