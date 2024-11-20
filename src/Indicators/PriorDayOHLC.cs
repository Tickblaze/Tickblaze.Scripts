namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Prior Day OHLC [PDOHLC]
/// </summary>
public partial class PriorDayOHLC : Indicator
{
	[Parameter("Start time"), NumericRange(0, 2359, 1)]
	public int StartTime { get; set; } = 930;

	[Parameter("End time"), NumericRange(0, 2359, 1)]
	public int EndTime { get; set; } = 1600;

	[Parameter("Show Open")]
	public bool ShowOpen { get; set; } = true;

	[Parameter("Show High")]
	public bool ShowHigh { get; set; } = true;

	[Parameter("Show Low")]
	public bool ShowLow { get; set; } = true;

	[Parameter("Show Close")]
	public bool ShowClose { get; set; } = true;

	[Plot("Open")]
	public PlotSeries Open { get; set; } = new(Color.Orange, LineStyle.Dash);

	[Plot("High")]
	public PlotSeries High { get; set; } = new(Color.Red, LineStyle.Dash);

	[Plot("Low")]
	public PlotSeries Low { get; set; } = new(Color.Blue, LineStyle.Dash);

	[Plot("Close")]
	public PlotSeries Close { get; set; } = new(Color.Yellow, LineStyle.Dash);

	private double _hh = double.MinValue;
	private double _ll = double.MaxValue;
	private double _o;
	private double _c;
	private string _dayIdStart = "";
	private string _dayIdEnded = "";
	private Tuple<double, double, double, double> _values = null;
	public PriorDayOHLC()
	{
		Name = "Prior Day OHLC";
		ShortName = "PDOHLC";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var time0 = ToTime(Bars[index].Time) / 100;
		var barTimeStr = Bars[index].Time.ToShortDateString();
		var isEndOfSession = false;
		//determine if EndTime was encountered today

		if (time0 >= EndTime && _dayIdEnded.CompareTo(barTimeStr) != 0)
		{
			_dayIdEnded = barTimeStr;
			isEndOfSession = true;
			if (_hh != double.MinValue)
			{
				_values = new Tuple<double, double, double, double>(_o, _hh, _ll, _c);
			}
		}

		if (StartTime < EndTime)
		{
			//Start time encountered, set variables
			if (time0 > StartTime && _dayIdStart.CompareTo(barTimeStr) != 0)
			{
				_dayIdStart = barTimeStr;
				_o = Bars[index].Open;
				_hh = Bars[index].High;
				_ll = Bars[index].Low;
				_c = Bars[index].Close;
			}
			//while we're between start and end, find HH and LL
			else if (time0 > StartTime && time0 <= EndTime)
			{
				_hh = Math.Max(_hh, Bars[index].High);
				_ll = Math.Min(_ll, Bars[index].Low);
				_c = Bars[index].Close;
			}
		}
		else if (StartTime > EndTime)
		{
			//Start time encountered today, set variables
			if (time0 > StartTime && _dayIdStart.CompareTo(barTimeStr) != 0)
			{
				_dayIdStart = barTimeStr;
				_o = Bars[index].Open;
				_hh = Bars[index].High;
				_ll = Bars[index].Low;
				_c = Bars[index].Close;
			}
			//while we're between start and end, find HH and LL
			else if (time0 > StartTime || time0 <= EndTime)
			{
				_hh = Math.Max(_hh, Bars[index].High);
				_ll = Math.Min(_ll, Bars[index].Low);
				_c = Bars[index].Close;
			}
		}
		else if (StartTime == EndTime)
		{
			//end and start are at the same time, so reset variables at that time
			if (isEndOfSession)
			{
				_o = Bars[index].Open;
				_hh = Bars[index].High;
				_ll = Bars[index].Low;
				_c = Bars[index].Close;
			}
			//continue to find HH and LL
			else
			{
				_hh = Math.Max(_hh, Bars[index].High);
				_ll = Math.Min(_ll, Bars[index].Low);
				_c = Bars[index].Close;
			}
		}

		//set plots to prior day ohlc
		if (_values != null)
		{
			Open[index] = 0;
			High[index] = 0;
			Low[index] = 0;
			Close[index] = 0;
			if (ShowOpen)
			{
				Open[index] = _values.Item1;
			}

			if (ShowHigh)
			{
				High[index] = _values.Item2;
			}

			if (ShowLow)
			{
				Low[index] = _values.Item3;
			}

			if (ShowClose)
			{
				Close[index] = _values.Item4;
			}
		}
	}

	private static int ToTime(DateTime t)
	{
		return t.Hour * 10000 + t.Minute * 100 + t.Second;
	}
}