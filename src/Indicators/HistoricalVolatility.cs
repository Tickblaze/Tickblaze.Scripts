namespace Tickblaze.Scripts.Indicators;

public sealed partial class HistoricalVolatility : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 20;

	[Parameter("Bar History"), NumericRange(1, int.MaxValue)]
	public int BarHistory { get; set; } = 252;

	[Plot("Resul")]
	public PlotSeries Result { get; set; } = new(Color.Orange);

	private DataSeries _logarithms;
	private StandardDeviation _standardDeviation;

	public HistoricalVolatility()
	{
		Name = "Historical Volatility";
		ShortName = "HV";
		ScalePrecision = 3;
	}

	protected override void Initialize()
	{
		_logarithms = new DataSeries();
		_standardDeviation = new StandardDeviation(_logarithms, Period, MovingAverageType.Simple);
	}

	protected override void Calculate(int index)
	{
		_logarithms[index] = Source.Count <= 1 ? 0 : Math.Log10(Source[index] / Source[index - 1]);
		Result[index] = _standardDeviation.Result[index] * Math.Sqrt(BarHistory);
	}
}
