namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// EaseOfMovement [EMV]
/// </summary>
public partial class EaseOfMovement : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Volume Divisor"), NumericRange(1, int.MaxValue)]
	public int VolumeDivisor { get; set; } = 10_000;

	[Plot("Result")]
	public PlotSeries Result { get; set; }

	private double _smoothFactor;

	public EaseOfMovement()
	{
		Name = "Ease of Movement";
		ShortName = "EMV";
		IsOverlay = false;
	}

	protected override void Initialize()
	{
		_smoothFactor = 2.0 / (Period + 1);
	}

	protected override void Calculate(int index)
	{
		if (index < 1)
		{
			Result[index] = 0;
			return;
		}

		var midPrice0 = (Bars[index].High + Bars[index].Low) / 2.0;
		var midPrice1 = (Bars[index - 1].High + Bars[index - 1].Low) / 2.0;
		var numerator = midPrice0 - midPrice1;
		var range = Math.Max(Bars[index].High - Bars[index].Low, Bars.Symbol.TickSize);
		var denominator = Bars[index].Volume / range;

		Result[index] = denominator == 0
			? Result[index - 1]
			: _smoothFactor * VolumeDivisor * (numerator / denominator) + (1 - _smoothFactor) * Result[index - 1];
	}
}