using System.ComponentModel;
using System.Diagnostics;

namespace Tickblaze.Scripts.Tests;

//[Browsable(false)]
public partial class NewSession : Indicator
{
	[Plot("Result")]
	public PlotSeries Result { get; set; }

	[Plot("Count")]
	public PlotSeries Count { get; set; }

	[Plot("Local Time")]
	public PlotSeries TimeLocal { get; set; }

	[Plot("UTC Time")]
	public PlotSeries TimeUtc { get; set; }

	[Parameter("Is Checked?")]
	public bool IsChecked { get; set; }

	[Parameter("Optional Input")]
	public int OptionalInput { get; set; }

	private IExchangeSession _lastSession;

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (IsChecked is false)
		{
			//parameters.Remove(nameof(OptionalInput));
			parameters[nameof(OptionalInput)].IsEnabled = false;
		}

		OptionalInput = IsChecked ? 1 : 0;

		return parameters;
	}

	protected override void Initialize()
	{
		Debugger.Break();
	}

	protected override void Calculate(int index)
	{
		if (index == 100)
		{
			Alert.ShowDialog(Api.Adapters.AlertType.Bad, "This is a test alert message.");
		}

		var time = Bars[index].Time;
		var timeLocal = time.ToLocalTime();
		var timeUtc = time.ToUniversalTime();

		Count[index] = double.IsNaN(Count[index]) ? 1 : Count[index] + 1;
		TimeLocal[index] = timeLocal.Hour * 100 + timeLocal.Minute;
		TimeUtc[index] = timeUtc.Hour * 100 + timeUtc.Minute;

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

	public override void OnRender(IDrawingContext context)
	{
		if (Bars is null || ChartScale is null || Chart is null)
		{
			return;
		}

		var close = Bars.Close[^1];
		var y = ChartScale.GetYCoordinateByValue(close);
		var x = Chart.GetXCoordinateByBarIndex(Bars.Count - 1);

		context.DrawEllipse(new Point(x, y), 10, 10, Color.Pink);
	}
}
