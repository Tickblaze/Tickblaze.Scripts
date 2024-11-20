namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Donchian Channel [DC]
/// </summary>
public partial class DonchianChannel : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue);

	[Plot("Middle")]
	public PlotSeries Middle { get; set; } = new(Color.Red);

	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue);

	private Maximum _maximum;
	private Minimum _minimum;

	public DonchianChannel()
	{
		Name = "Donchian Channel";
		ShortName = "DC";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_maximum = new Maximum(Bars.High, Period);
		_minimum = new Minimum(Bars.Low, Period);
	}

	protected override void Calculate(int index)
	{
		var maximum = _maximum[index];
		var minimum = _minimum[index];

		Upper[index] = maximum;
		Lower[index] = minimum;
		Middle[index] = (maximum + minimum) / 2;
	}
}
