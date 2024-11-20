using System.ComponentModel;

namespace Tickblaze.Scripts.Indicators;

[Browsable(false)]
public partial class NewSession : Indicator
{
	[Plot("Result")]
	public PlotSeries Result { get; set; }

	[Plot("Count")]
	public PlotSeries Count { get; set; }

	[Plot("Time")]
	public PlotSeries Time { get; set; }

	private IExchangeSession _lastSession;

	protected override void Calculate(int index)
	{
		var time = Bars[index].Time;

		Count[index] = double.IsNaN(Count[index]) ? 1 : Count[index] + 1;
		Time[index] = time.Hour * 100 + time.Minute;

		var session = Bars.Symbol.ExchangeCalendar.GetSession(time);
		if (session != _lastSession)
		{
			_lastSession = session;

			Result[index] = 1;
		}
		else
		{
			Result[index] = 0;
		}
	}
}
