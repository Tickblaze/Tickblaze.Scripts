namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Average Directional Movement Index Rating [ADXR]
/// </summary>
public partial class AverageDirectionalMovementIndexRating : Indicator
{
	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 14;

	[Parameter("Interval"), NumericRange(1, 999, 1)]
	public int Interval { get; set; } = 10;

	[Plot("Result")]
	public PlotSeries Result { get; set; }

	[Plot("Lower")]
	public PlotLevel Lower { get; set; } = new(25, "#80787b86", LineStyle.Dash, 1);

	[Plot("Upper")]
	public PlotLevel Upper { get; set; } = new(75, "#80787b86", LineStyle.Dash, 1);

	private AverageDirectionalMovementIndex _adx;

	public AverageDirectionalMovementIndexRating()
	{
		Name = "Average Directional Movement Index Rating";
		ShortName = "ADXR";
	}

	protected override void Initialize()
	{
		_adx = new AverageDirectionalMovementIndex(Period);
	}

	protected override void Calculate(int index)
	{
		Result[index] = index > Interval ? (_adx[index] + _adx[index - Interval]) / 2 : 0;
	}
}
