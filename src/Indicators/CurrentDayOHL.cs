namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// CurrentDayOHL [CDOHL]
/// </summary>
public partial class CurrentDayOHL : Indicator
{
	[Plot("Open")]
	public PlotSeries Open { get; set; } = new(Color.Orange, LineStyle.Dash);

	[Plot("High")]
	public PlotSeries High { get; set; } = new(Color.Red, LineStyle.Dash);

	[Plot("Low")]
	public PlotSeries Low { get; set; } = new(Color.Blue, LineStyle.Dash);

	private IExchangeSession _lastSession;

	public CurrentDayOHL()
	{
		Name = "Current Day OHL";
		ShortName = "CDOHL";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		if (index > 0)
		{
			Open[index] = Open[index - 1];
			High[index] = High[index - 1];
			Low[index] = Low[index - 1];
		}

		var bar = Bars[index];
		var currentSession = Bars.Symbol.ExchangeCalendar.GetSession(bar.Time);

		var isNewSession = _lastSession is null || _lastSession != currentSession;
		if (isNewSession)
		{
			_lastSession = currentSession;

			Open[index] = bar.Open;
			High[index] = bar.High;
			Low[index] = bar.Low;
		}
		else
		{
			High[index] = Math.Max(High[index], bar.High);
			Low[index] = Math.Min(Low[index], bar.Low);
		}
	}
}