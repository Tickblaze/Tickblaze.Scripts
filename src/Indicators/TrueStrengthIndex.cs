namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// True Strength Index [TSI]
/// </summary>
public partial class TrueStrengthIndex : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Fast Period"), NumericRange(1, int.MaxValue)]
	public int FastPeriod { get; set; } = 3;

	[Parameter("Slow Period"), NumericRange(1, int.MaxValue)]
	public int SlowPeriod { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	private double _constant1, _constant2, _constant3, _constant4;
	private DataSeries _fastEma, _slowEma, _fastAbsEma, _slowAbsEma;

	public TrueStrengthIndex()
	{
		Name = "True Strength Index";
		ShortName = "TSI";
		IsOverlay = false;
	}

	protected override void Initialize()
	{
		_constant1 = 2.0 / (1 + SlowPeriod);
		_constant2 = 1 - (2.0 / (1 + SlowPeriod));
		_constant3 = 2.0 / (1 + FastPeriod);
		_constant4 = 1 - (2.0 / (1 + FastPeriod));

		_fastEma = new();
		_slowEma = new();
		_fastAbsEma = new();
		_slowAbsEma = new();
	}

	protected override void Calculate(int index)
	{
		if (index == 0)
		{
			_fastAbsEma[index] = 0;
			_fastEma[index] = 0;
			_slowAbsEma[index] = 0;
			_slowEma[index] = 0;

			Result[index] = 0;
		}
		else
		{
			var momentum = Source[index] - Source[index - 1];

			_slowEma[index] = momentum * _constant1 + _constant2 * _slowEma[index - 1];
			_fastEma[index] = _slowEma[index] * _constant3 + _constant4 * _fastEma[index - 1];
			_slowAbsEma[index] = Math.Abs(momentum) * _constant1 + _constant2 * _slowAbsEma[index - 1];
			_fastAbsEma[index] = _slowAbsEma[index] * _constant3 + _constant4 * _fastAbsEma[index - 1];

			Result[index] = _fastAbsEma[index] == 0 ? 0 : 100 * _fastEma[index] / _fastAbsEma[index];
		}
	}
}
