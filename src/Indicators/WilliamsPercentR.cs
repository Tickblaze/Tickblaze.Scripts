namespace Tickblaze.Scripts.Indicators;

public partial class WilliamsPercentR : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Williams %R")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	[Plot("Overbought")]
	public PlotLevel OverboughtLevel { get; set; } = new(-20, Color.Red);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(-80, Color.Green);

	private Maximum _maximum;
	private Minimum _minimum;

	public WilliamsPercentR()
	{
		Name = "WIlliams %R";
		ShortName = "W%";
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

		Result[index] = (maximum - Bars.Close[index]) / (maximum - minimum) * -100;
	}
}
