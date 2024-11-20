using System.ComponentModel;

namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Pivot Points
/// </summary>
[Browsable(false)]
public partial class CamarillaPivots : Indicator
{
	[Parameter("Start time"), NumericRange(0, 2359, 1)]
	public int StartTime { get; set; } = 930;

	[Parameter("End time"), NumericRange(0, 2359, 1)]
	public int EndTime { get; set; } = 1600;

	[Plot("R4")]
	public PlotSeries R4 { get; set; } = new("#ff2200", LineStyle.Dash);

	[Plot("R3")]
	public PlotSeries R3 { get; set; } = new("#ff6600", LineStyle.Dash);

	[Plot("R2")]
	public PlotSeries R2 { get; set; } = new("#ff9900", LineStyle.Dash);

	[Plot("R1")]
	public PlotSeries R1 { get; set; } = new("#ffdd00", LineStyle.Dash, 1);

	[Plot("S1")]
	public PlotSeries S1 { get; set; } = new("#00ff00", LineStyle.Dash);

	[Plot("S2")]
	public PlotSeries S2 { get; set; } = new("#00aa00", LineStyle.Dash);

	[Plot("S3")]
	public PlotSeries S3 { get; set; } = new("#006600", LineStyle.Dash);

	[Plot("S4")]
	public PlotSeries S4 { get; set; } = new("#003311", LineStyle.Dash);

	private double _highestHigh = double.MinValue;
	private double _lowestLow = double.MaxValue;
	private double _open;
	private double _close;
	private string _dayIdStart;
	private string _dayIdEnded;
	private Tuple<double, double, double, double> _values;

	public CamarillaPivots()
	{
		Name = "Camarilla Pivots";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var timeInt = ToTime(Bars[index].Time) / 100;
		var barTimeStr = Bars[index].Time.ToShortDateString();
		var isEndOfSession = false;

		if (timeInt >= EndTime && _dayIdEnded.CompareTo(barTimeStr) != 0)//determine if EndTime was encountered today
		{
			_dayIdEnded = barTimeStr;
			isEndOfSession = true;
			if (_highestHigh != double.MinValue)
			{
				_values = new Tuple<double, double, double, double>(_open, _highestHigh, _lowestLow, _close);
			}
		}

		if (StartTime < EndTime)
		{
			if (timeInt > StartTime && _dayIdStart.CompareTo(barTimeStr) != 0)//Start time encountered, set variables
			{
				_dayIdStart = barTimeStr;
				_open = Bars[index].Open;
				_highestHigh = Bars[index].High;
				_lowestLow = Bars[index].Low;
				_close = Bars[index].Close;
			}
			else if (timeInt > StartTime && timeInt <= EndTime)//while we're between start and end, find HH and LL
			{
				_highestHigh = Math.Max(_highestHigh, Bars[index].High);
				_lowestLow = Math.Min(_lowestLow, Bars[index].Low);
				_close = Bars[index].Close;
			}
		}
		else if (StartTime > EndTime)
		{
			if (timeInt > StartTime && _dayIdStart.CompareTo(barTimeStr) != 0)//Start time encountered today, set variables
			{
				_dayIdStart = barTimeStr;
				_open = Bars[index].Open;
				_highestHigh = Bars[index].High;
				_lowestLow = Bars[index].Low;
				_close = Bars[index].Close;
			}
			else if (timeInt > StartTime || timeInt <= EndTime)//while we're between start and end, find HH and LL
			{
				_highestHigh = Math.Max(_highestHigh, Bars[index].High);
				_lowestLow = Math.Min(_lowestLow, Bars[index].Low);
				_close = Bars[index].Close;
			}
		}
		else if (StartTime == EndTime)
		{
			if (isEndOfSession)
			{
				_open = Bars[index].Open;
				_highestHigh = Bars[index].High;
				_lowestLow = Bars[index].Low;
				_close = Bars[index].Close;
			}
			else
			{
				_highestHigh = Math.Max(_highestHigh, Bars[index].High);
				_lowestLow = Math.Min(_lowestLow, Bars[index].Low);
				_close = Bars[index].Close;
			}
		}

		R4[index] = Bars[index].Close;
		R3[index] = R4[index];
		R2[index] = R4[index];
		R1[index] = R4[index];
		S1[index] = R4[index];
		S2[index] = R4[index];
		S3[index] = R4[index];
		S4[index] = R4[index];

		if (_values != null)
		{
			var range = _values.Item2 - _values.Item3;

			R1[index] = _values.Item4 + range * 1.1 / 12;
			S1[index] = _values.Item4 - range * 1.1 / 12;
			R2[index] = _values.Item4 + range * 1.1 / 6;
			S2[index] = _values.Item4 - range * 1.1 / 6;
			R3[index] = _values.Item4 + range * 1.1 / 4;
			S3[index] = _values.Item4 - range * 1.1 / 4;
			R4[index] = _values.Item4 + range * 1.1 / 2;
			S4[index] = _values.Item4 - range * 1.1 / 2;
		}
	}

	private static int ToTime(DateTime t)
	{
		return t.Hour * 10000 + t.Minute * 100 + t.Second;
	}
}