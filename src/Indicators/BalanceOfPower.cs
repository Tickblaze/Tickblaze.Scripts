namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Balance Of Power [BOP]
/// </summary>
public partial class BalanceOfPower : Indicator
{
	[Parameter("Smoothing Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Parameter("Smooth Period"), NumericRange(1, int.MaxValue)]
	public int SmoothingPeriod { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.DeepPurple);

	[Plot("Zero")]
	public PlotLevel ZeroLevel { get; set; } = new(0, Color.Gray, LineStyle.Dash);

	private DataSeries _balanceOfPower;
	private MovingAverage _movingAverage;

	public BalanceOfPower()
	{
		Name = "Balance Of Power";
		ShortName = "BOP";
	}

	protected override void Initialize()
	{
		_balanceOfPower = new DataSeries();
		_movingAverage = new MovingAverage(_balanceOfPower, SmoothingPeriod, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		var range = bar.High - bar.Low;

		_balanceOfPower[index] = range > 0 ? (bar.Close - bar.Open) / range : 0;

		Result[index] = _movingAverage[index];
	}
}