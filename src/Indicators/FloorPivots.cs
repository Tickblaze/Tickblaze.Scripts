using System.ComponentModel;
namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Floor Pivots [FPiv]
/// </summary>
[Browsable(false)]
public partial class FloorPivots : Indicator
{
	[Parameter("Start time"), NumericRange(0, 2359, 1)]
	public int StartTime { get; set; } = 930;

	[Parameter("End time"), NumericRange(0, 2359, 1)]
	public int EndTime { get; set; } = 1600;

	[Plot("PP")]
	public PlotSeries Pp { get; set; } = new("#ffaa00", LineStyle.Solid, 1);

	[Plot("R1")]
	public PlotSeries R1 { get; set; } = new("#000", LineStyle.Dash, 1);

	[Plot("R2")]
	public PlotSeries R2 { get; set; } = new("#000", LineStyle.Dash, 1);

	[Plot("R3")]
	public PlotSeries R3 { get; set; } = new("#ff3311", LineStyle.Dash, 1);

	[Plot("S1")]
	public PlotSeries S1 { get; set; } = new("#000", LineStyle.Dash, 1);

	[Plot("S2")]
	public PlotSeries S2 { get; set; } = new("#000", LineStyle.Dash, 1);

	[Plot("S3")]
	public PlotSeries S3 { get; set; } = new("#ff3311", LineStyle.Dash, 1);

	private double _highestHigh = double.MinValue;
	private double _lowestLow = double.MaxValue;
	private double _close;
	private double _r1, _r2, _r3, _pp, _s1, _s2, _s3 = double.MinValue;
	private string _dayIdStart = "";
	private string _dayIdEnded = "";

	public FloorPivots()
	{
		Name = "Floor Pivots";
		ShortName = "FPiv";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var time0 = ToTime(Bars[index].Time) / 100;
		var barTimeStr = Bars[index].Time.ToShortDateString();
		var isEndOfSession = false;
		if (time0 >= EndTime && _dayIdEnded.CompareTo(barTimeStr) != 0)
		{
			_dayIdEnded = barTimeStr;
			isEndOfSession = true;
			if (_highestHigh != double.MinValue)
			{
				_pp = (_highestHigh + _lowestLow + _close) / 3;
				_s1 = 2 * _pp - _highestHigh;
				_r1 = 2 * _pp - _lowestLow;
				_s2 = _pp - (_highestHigh - _lowestLow);
				_r2 = _pp + (_highestHigh - _lowestLow);
				_s3 = _pp - 2 * (_highestHigh - _lowestLow);
				_r3 = _pp + 2 * (_highestHigh - _lowestLow);
			}
		}

		if (_pp != double.MinValue)
		{
			R3[index] = _r3;
			R2[index] = _r2;
			R1[index] = _r1;
			Pp[index] = _pp;
			S1[index] = _s1;
			S2[index] = _s2;
			S3[index] = _s3;
		}
		else
		{
			R3[index] = R2[index] = R1[index] = Pp[index] = S1[index] = S2[index] = S3[index] = Bars[index].Close;
		}

		if (StartTime < EndTime)
		{
			if (time0 > StartTime && _dayIdStart.CompareTo(barTimeStr) != 0)
			{
				_dayIdStart = barTimeStr;
				_highestHigh = Bars[index].High;
				_lowestLow = Bars[index].Low;
				_close = Bars[index].Close;
			}
			else if (time0 > StartTime && time0 <= EndTime)
			{
				_highestHigh = Math.Max(_highestHigh, Bars[index].High);
				_lowestLow = Math.Min(_lowestLow, Bars[index].Low);
				_close = Bars[index].Close;
			}
		}
		else if (StartTime > EndTime)
		{
			if (time0 > StartTime && _dayIdStart.CompareTo(barTimeStr) != 0)
			{
				_dayIdStart = barTimeStr;
				_highestHigh = Bars[index].High;
				_lowestLow = Bars[index].Low;
				_close = Bars[index].Close;
			}
			else if (time0 > StartTime || time0 <= EndTime)
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
	}

	private static int ToTime(DateTime t)
	{
		return t.Hour * 10000 + t.Minute * 100 + t.Second;
	}
}