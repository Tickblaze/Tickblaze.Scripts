namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Stochastic Oscillator [SO]
/// </summary>
public partial class StochasticOscillator : Indicator
{
	[Parameter("%K Periods"), NumericRange(1, int.MaxValue)]
	public int KPeriods { get; set; } = 9;

	[Parameter("%K Slowing"), NumericRange(1, int.MaxValue)]
	public int KSlowing { get; set; } = 3;

	[Parameter("%D Periods"), NumericRange(1, int.MaxValue)]
	public int DPeriods { get; set; } = 9;

	[Parameter("MA Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Plot("%K")]
	public PlotSeries PercentK { get; set; } = new(Color.Blue);

	[Plot("%D")]
	public PlotSeries PercentD { get; set; } = new(Color.Orange, LineStyle.Dash);

	[Plot("Overbought")]
	public PlotLevel OverboughtLevel { get; set; } = new(80, Color.Red, LineStyle.Dash, 1);

	[Plot("Middle level")]
	public PlotLevel MiddleLevel { get; set; } = new(50, "#80808080", LineStyle.Dash, 1);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(20, Color.Green, LineStyle.Dash, 1);

	private DataSeries _fastK;
	private Maximum _maximum;
	private Minimum _minimum;
	private MovingAverage _slowK, _averageOnSlowK;

	public StochasticOscillator()
	{
		Name = "Stochastic Oscillator";
		ShortName = "SO";
	}

	protected override void Initialize()
	{
		_fastK = new DataSeries();
		_maximum = new Maximum(Bars.High, KPeriods);
		_minimum = new Minimum(Bars.Low, KPeriods);
		_slowK = new MovingAverage(_fastK, KSlowing, SmoothingType);
		_averageOnSlowK = new MovingAverage(_slowK.Result, DPeriods, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		var minimum = _minimum[index];
		var maximum = _maximum[index];

		_fastK[index] = (Bars.Close[index] - minimum) / (maximum - minimum) * 100;

		PercentK[index] = _slowK[index];
		PercentD[index] = _averageOnSlowK[index];
	}
}
