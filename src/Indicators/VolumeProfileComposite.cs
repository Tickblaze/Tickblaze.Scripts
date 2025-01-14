using System.ComponentModel;
using System.Globalization;
using Tickblaze.Scripts.Drawings;

namespace Tickblaze.Scripts.Indicators;

public partial class VolumeProfileComposite : Indicator, VolumeProfile.ISettings
{
	[Parameter("Composition", Description = "Composition type (daily/weekly/monthly)")]
	public CompositionType Composition { get; set; } = CompositionType.Daily;

	[Parameter("Data Source", Description = "Data source for the volume profile")]
	public VolumeProfile.SourceDataType SourceData { get; set; } = VolumeProfile.SourceDataType.Chart;

	[Parameter("Histo Size Type", Description = "Determines how histogram rows are calculated (by count or ticks)")]
	public VolumeProfile.RowsLayoutType RowsLayout { get; set; } = VolumeProfile.RowsLayoutType.Count;

	[NumericRange(1, int.MaxValue)]
	[Parameter("Histo Size Value", Description = "Defines the size of the histogram rows")]
	public int RowsSize { get; set; } = 24;

	public const string StyleGroupName = "Style";

	[NumericRange(0, 100)]
	[Parameter("Histo Width %", Description = "Width of the histogram as a percentage of box width", GroupName = StyleGroupName)]
	public double RowsWidthPercent { get; set; } = 30;

	[Parameter("Histo Location", Description = "Location of histogram (left or right side of box)", GroupName = StyleGroupName)]
	public VolumeProfile.PlacementType RowsPlacement { get; set; } = VolumeProfile.PlacementType.Left;

	[NumericRange(0, 100)]
	[Parameter("Value Area %", Description = "Percentage of total volume considered in the value area", GroupName = StyleGroupName)]
	public double ValueAreaPercent { get; set; } = 70;

	[Parameter("Value Area Color", Description = "Color of the value area", GroupName = StyleGroupName)]
	public Color ValueAreaColor { get; set; } = "#bf808080";

	[Parameter("Above Value Area Color", Description = "Color of area above the value area", GroupName = StyleGroupName)]
	public Color ValueAreaAboveColor { get; set; } = "#80ff0000";

	[Parameter("Below Value Area Color", Description = "Color of area below the value area", GroupName = StyleGroupName)]
	public Color ValueAreaBelowColor { get; set; } = "#80ff0000";

	[Parameter("Show Outline?", Description = "Show/Hide the outline", GroupName = StyleGroupName)]
	public bool BoxVisible { get; set; }

	[Parameter("Outline Color", Description = "Color of the volume profile outline box", GroupName = StyleGroupName)]
	public Color BoxLineColor { get; set; } = "#80ffffff";

	[Parameter("Outline Thickness", Description = "Thickness of the volume profile outline box", GroupName = StyleGroupName)]
	public int BoxLineThickness { get; set; } = 1;

	[Parameter("Outline Style", Description = "Style of the volume profile outline box (solid, dashed, etc.)", GroupName = StyleGroupName)]
	public LineStyle BoxLineStyle { get; set; } = LineStyle.Dot;

	[Parameter("Show VAH Line?", Description = "Show/Hide the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public bool VahLineVisible { get; set; } = false;

	[Parameter("VAH Line Color", Description = "Color of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public Color VahLineColor { get; set; } = Color.White;

	[Parameter("VAH Line Thickness", Description = "Thickness of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public int VahLineThickness { get; set; } = 2;

	[Parameter("VAH Line Style", Description = "Style of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public LineStyle VahLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show VAL Line?", Description = "Show/Hide the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public bool ValLineVisible { get; set; } = false;

	[Parameter("VAL Line Color", Description = "Color of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public Color ValLineColor { get; set; } = Color.White;

	[Parameter("VAL Line Thickness", Description = "Thickness of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public int ValLineThickness { get; set; } = 2;

	[Parameter("VAL Line Style", Description = "Style of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public LineStyle ValLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show POC Line?", Description = "Show/Hide the Point of Control (POC) line", GroupName = StyleGroupName)]
	public bool PocLineVisible { get; set; } = true;

	[Parameter("POC Line Color", Description = "Color of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public Color PocLineColor { get; set; } = Color.Yellow;

	[Parameter("POC Line Thickness", Description = "Thickness of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public int PocLineThickness { get; set; } = 2;

	[Parameter("POC Line Style", Description = "Style of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public LineStyle PocLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show VAH/VAL/POC Prices?", Description = "Displays prices for VAH, VAL, and POC levels", GroupName = StyleGroupName)]
	public bool ShowPrices { get; set; } = false;

	[Parameter("Font", Description = "Font for displaying VAH/VAL/POC prices", GroupName = StyleGroupName)]
	public Font Font { get; set; } = new("Arial", 12);

	[Parameter("VWAP Enabled?", Description = "Show/Hide the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public bool VwapEnabled { get; set; } = false;

	[Parameter("VWAP Line Color", Description = "Color of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public Color VwapLineColor { get; set; } = Color.Blue;

	[Parameter("VWAP Line Thickness", Description = "Thickness of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public int VwapLineThickness { get; set; } = 1;

	[Parameter("VWAP Line Style", Description = "Style of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public LineStyle VwapLineStyle { get; set; } = LineStyle.Solid;

	public enum CompositionType
	{
		Daily,
		Weekly,
		Monthly,
		Yearly
	}

	private BarSeries _bars;
	private IExchangeSession _lastSession;
	private VolumeProfile.Area<VolumeProfileComposite> _currentArea;
	private readonly List<VolumeProfile.Area<VolumeProfileComposite>> _areas = [];

	public VolumeProfileComposite()
	{
		Name = "Volume Profile - Composite";
		IsOverlay = true;
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		parameters[nameof(RowsSize)].Attributes.Name = RowsLayout is VolumeProfile.RowsLayoutType.Count
			? "Histo Rows Count"
			: "Histo Ticks Size";

		if (BoxVisible is false)
		{
			parameters.Remove(nameof(BoxLineColor));
			parameters.Remove(nameof(BoxLineThickness));
			parameters.Remove(nameof(BoxLineStyle));
		}

		if (VahLineVisible is false)
		{
			parameters.Remove(nameof(VahLineColor));
			parameters.Remove(nameof(VahLineThickness));
			parameters.Remove(nameof(VahLineStyle));
		}

		if (ValLineVisible is false)
		{
			parameters.Remove(nameof(ValLineColor));
			parameters.Remove(nameof(ValLineThickness));
			parameters.Remove(nameof(ValLineStyle));
		}

		if (PocLineVisible is false)
		{
			parameters.Remove(nameof(PocLineColor));
			parameters.Remove(nameof(PocLineThickness));
			parameters.Remove(nameof(PocLineStyle));
		}

		if (VwapEnabled is false)
		{
			parameters.Remove(nameof(VwapLineColor));
			parameters.Remove(nameof(VwapLineThickness));
			parameters.Remove(nameof(VwapLineStyle));
		}

		if (ShowPrices is false)
		{
			parameters.Remove(nameof(Font));
		}

		return parameters;
	}

	protected override void Initialize()
	{
		_bars = VolumeProfile.TryGetDataSeriesRequest(this, out var request) ? GetBarSeries(request) : Bars;
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		var time = bar.Time;

		var session = Bars.Symbol.ExchangeCalendar.GetSession(time);
		if (session != _lastSession)
		{
			var isNewProfile = _lastSession is null;
			if (isNewProfile is false)
			{
				var lastSessionStart = _lastSession.StartExchangeDateTime;
				var sessionStart = session!.StartExchangeDateTime;

				isNewProfile = Composition switch
				{
					CompositionType.Daily => true,
					CompositionType.Weekly => IsNewWeek(lastSessionStart, sessionStart),
					CompositionType.Monthly => lastSessionStart.Month != sessionStart.Month,
					CompositionType.Yearly => lastSessionStart.Year < sessionStart.Year,
					_ => throw new ArgumentOutOfRangeException()
				};
			}

			if (isNewProfile)
			{
				_currentArea = new VolumeProfile.Area<VolumeProfileComposite>(this, index, index, _bars);
				_areas.Add(_currentArea);
			}

			_lastSession = session;
		}

		if (_currentArea is not null)
		{
			_currentArea.ToIndex = index;
		}
	}

	private static bool IsNewWeek(DateTime time1, DateTime time2)
	{
		var week1 = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time1, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
		var week2 = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time2, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

		return week1 != week2;
	}

	public override void OnRender(IDrawingContext context)
	{
		foreach (var area in _areas)
		{
			area.Render(context);
		}
	}
}