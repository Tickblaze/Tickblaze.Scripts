namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Money Flow Index [MFI]
/// </summary>
public partial class MoneyFlowIndex : Indicator
{
	[Parameter("Period"), NumericRange(2, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	[Plot("Overbought")]
	public PlotLevel OverboughtLevel { get; set; } = new(80, Color.Red, LineStyle.Dash);

	[Plot("Middle")]
	public PlotLevel MiddleLevel { get; set; } = new(50, "#80787b86", LineStyle.Dash);

	[Plot("Oversold")]
	public PlotLevel OversoldLevel { get; set; } = new(20, Color.Green, LineStyle.Dash);

	public MoneyFlowIndex()
	{
		Name = "Money Flow Index";
		ShortName = "MFI";
	}

	protected override void Calculate(int index)
	{
		if (index < Period)
		{
			return;
		}

		var num = CalculateMoneyFlow(index, (double current, double previous) => current > previous);
		var num2 = CalculateMoneyFlow(index, (double current, double previous) => current < previous);
		var num3 = num / num2;

		Result[index] = 100.0 - 100.0 / (1.0 + num3);
	}

	private double CalculateMoneyFlow(int index, Func<double, double, bool> addCondition)
	{
		var num = 0.0;

		for (var i = 0; i < Period; i++)
		{
			var num2 = index - i;
			var num3 = Bars.TypicalPrice[num2];
			var arg = Bars.TypicalPrice[num2 - 1];

			if (addCondition(num3, arg))
			{
				num += num3 * Bars.Volume[num2];
			}
		}

		return num;
	}
}
