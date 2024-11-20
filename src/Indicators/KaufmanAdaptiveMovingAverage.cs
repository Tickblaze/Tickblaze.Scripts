namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Kaufman Adaptive Moving Average [KAMA]
/// </summary>
public partial class KaufmanAdaptiveMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Fast"), NumericRange(1, 999, 1)]
	public int PeriodFast { get; set; } = 2;

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 10;

	[Parameter("Slow"), NumericRange(1, 999, 1)]
	public int PeriodSlow { get; set; } = 30;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#2962ff", LineStyle.Solid, 1);

	private List<double> _diffSeries = [];
	private double _fastCf;
	private double _slowCf;
	private double _sum;
	private int _priorIndex = -1;

	public KaufmanAdaptiveMovingAverage()
	{
		Name = "Kaufman Adaptive Moving Average";
		ShortName = "KAMA";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_fastCf = 2.0 / (PeriodFast + 1);
		_slowCf = 2.0 / (PeriodSlow + 1);
	}

	protected override void Calculate(int index)
	{
		var input0 = Source[index];

		Result[index] = input0;

		if (index < 1)
		{
			return;
		}

		var input1 = Source[index - 1];

		if (_priorIndex != index)
		{
			_priorIndex = index;
			_diffSeries.Add(index > 0 ? Math.Abs(input0 - input1) : input0);

			while (_diffSeries.Count > Period)
			{
				_diffSeries.RemoveAt(0);
			}
		}
		else
		{
			_diffSeries[^1] = index > 0 ? Math.Abs(input0 - input1) : input0;
		}

		if (index < Period)
		{
			Result[index] = Source[index];
			return;
		}

		var noise = _diffSeries.Sum();
		if (noise == 0)
		{
			Result[index] = Result[index - 1];
			return;
		}

		var signal = Math.Abs(input0 - Source[index - Period]);
		var value1 = Result[index - 1];

		Result[index] = value1 + Math.Pow(signal / noise * (_fastCf - _slowCf) + _slowCf, 2) * (input0 - value1);
	}
}