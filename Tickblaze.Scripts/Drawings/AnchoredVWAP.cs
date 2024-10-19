
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Drawings;

public sealed class AnchoredVWAP : Drawing
{
	[Parameter("Extend to current bar")]
	public bool ExtendToCurrentBar { get; set; } = false;

	[Parameter("VWAP Line Color")]
	public Color VWAPLineColor { get => _bandSettingsDict[VWAPIds.VWAP].Color; set => _bandSettingsDict[VWAPIds.VWAP].Color = value; }

	[Parameter("VWAP Line Thickness"), NumericRange(1, 5)]
	public int VWAPLineThickness { get => _bandSettingsDict[VWAPIds.VWAP].Thickness; set => _bandSettingsDict[VWAPIds.VWAP].Thickness = value; }

	[Parameter("VWAP Line Style")]
	public LineStyle VWAPLineStyle { get => _bandSettingsDict[VWAPIds.VWAP].LineStyle; set => _bandSettingsDict[VWAPIds.VWAP].LineStyle = value; }

	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Mult { get => _bandSettingsDict[VWAPIds.Band1].Multiplier; set => _bandSettingsDict[VWAPIds.Band1].Multiplier = value; }

	[Parameter("Band 1 Color")]
	public Color Band1Color { get => _bandSettingsDict[VWAPIds.Band1].Color; set => _bandSettingsDict[VWAPIds.Band1].Color = value; }

	[Parameter("Band 1 Line Thickness"), NumericRange(0, 5)]
	public int Band1LineThickness { get => _bandSettingsDict[VWAPIds.Band1].Thickness; set => _bandSettingsDict[VWAPIds.Band1].Thickness = value; }

	[Parameter("Band 1 Line Style")]
	public LineStyle Band1LineStyle { get => _bandSettingsDict[VWAPIds.Band1].LineStyle; set => _bandSettingsDict[VWAPIds.Band1].LineStyle = value; }

	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Mult { get => _bandSettingsDict[VWAPIds.Band2].Multiplier; set => _bandSettingsDict[VWAPIds.Band2].Multiplier = value; }

	[Parameter("Band 2 Color")]
	public Color Band2Color { get => _bandSettingsDict[VWAPIds.Band2].Color; set => _bandSettingsDict[VWAPIds.Band2].Color = value; }

	[Parameter("Band 2 Line Thickness"), NumericRange(0, 5)]
	public int Band2LineThickness { get => _bandSettingsDict[VWAPIds.Band2].Thickness; set => _bandSettingsDict[VWAPIds.Band2].Thickness = value; }

	[Parameter("Band 2 Line Style")]
	public LineStyle Band2LineStyle { get => _bandSettingsDict[VWAPIds.Band2].LineStyle; set => _bandSettingsDict[VWAPIds.Band2].LineStyle = value; }

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Mult { get => _bandSettingsDict[VWAPIds.Band3].Multiplier; set => _bandSettingsDict[VWAPIds.Band3].Multiplier = value; }

	[Parameter("Band 3 Color")]
	public Color Band3Color { get => _bandSettingsDict[VWAPIds.Band3].Color; set => _bandSettingsDict[VWAPIds.Band3].Color = value; }

	[Parameter("Band 3 Line Thickness"), NumericRange(0, 5)]
	public int Band3LineThickness { get => _bandSettingsDict[VWAPIds.Band3].Thickness; set => _bandSettingsDict[VWAPIds.Band3].Thickness = value; }

	[Parameter("Band 3 Line Style")]
	public LineStyle Band3LineStyle { get => _bandSettingsDict[VWAPIds.Band3].LineStyle; set => _bandSettingsDict[VWAPIds.Band3].LineStyle = value; }

	[Parameter("Anchor line Color")]
	public Color AnchorLineColor { get; set; } = Color.DimGray;

	[Parameter("Anchor Line Thickness"), NumericRange(0, 5)]
	public int AnchorLineThickness { get; set; } = 1;

	[Parameter("Anchor Line Style")]
	public LineStyle AnchorLineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => 2;

	private readonly Dictionary<VWAPIds, BandSettings> _bandSettingsDict = new()
	{
		[VWAPIds.VWAP] = new BandSettings
		{
			Color = Color.Cyan
		},
		[VWAPIds.Band1] = new BandSettings
		{
			Multiplier = 0.75,
			Color = Color.Green
		},
		[VWAPIds.Band2] = new BandSettings
		{
			Multiplier = 1.75,
			Color = Color.Yellow
		},
		[VWAPIds.Band3] = new BandSettings
		{
			Multiplier = 2.75,
			Color = Color.Red
		}
	};

	public AnchoredVWAP()
	{
		Name = "Anchored VWAP";
	}

	private readonly Dictionary<VWAPIds, double> _priorLowerY = [];
	private readonly Dictionary<VWAPIds, double> _priorUpperY = [];
	public override void OnRender(IDrawingContext context)
	{
		if (AnchorLineThickness > 0)
		{
			context.DrawLine(Points[0], Points[1], AnchorLineColor, AnchorLineThickness, AnchorLineStyle);

			foreach (var pt in Points)
			{
				var isHigh = pt == Points[0];
				var bar = Chart.GetBarIndexByXCoordinate(Points[0].X);
				context.DrawText(Points[0], $"{(isHigh ? "High" : "Low")} @ {bar}: {Bars.Symbol.FormatPrice(isHigh ? Bars[bar].High : Bars[bar].Low)}  time: {Bars[bar].Time.ToLocalTime()}", Color.White);
			}
		}

		var leftIndex = Math.Max(0, Math.Min(Bars.Count - 1, Chart.GetBarIndexByXCoordinate(Math.Min(Points[0].X, Points[1].X))));
		var rightIndex = Math.Max(1, ExtendToCurrentBar ? (Bars == null ? 100 : Bars.Count - 1) : Math.Min(Bars.Count - 1, Chart.GetBarIndexByXCoordinate(Math.Max(Points[0].X, Points[1].X))));

		var volumeSum = 0.0;
		var typicalVolumeSum = 0.0;
		var varianceSum = 0.0;
		var pointL = new Point(0, 0);
		var pointR = new Point(0, 0);

		if (leftIndex >= rightIndex - 1)
		{
			return;
		}

		// Gap bars are null
		var validBarIndexes = Enumerable.Range(leftIndex, rightIndex - leftIndex)
			.Where(i => Bars[i] != null)
			.ToArray();

		for (var i = 0; i < validBarIndexes.Length; i++)
		{
			var barIndex = validBarIndexes[i];
			var bar = Bars[barIndex];
			var volume = bar.Volume;
			var typicalPrice = (bar.High + bar.Low + bar.Close) / 3;
			typicalVolumeSum += volume * typicalPrice;

			if (i == 0)
			{
				volumeSum = volume;
				pointR = new Point(Chart.GetXCoordinateByBarIndex(barIndex), ChartScale.GetYCoordinateByValue(typicalPrice));

				//left-edge of all plots start out at the same Y pixel
				foreach (var id in Enum.GetValues<VWAPIds>())
				{
					_priorUpperY[id] = _priorLowerY[id] = pointR.Y;
				}

				continue;
			}

			volumeSum += volume;
			var curVWAP = typicalVolumeSum / volumeSum;
			var diff = typicalPrice - curVWAP;
			varianceSum += diff * diff;
			var deviation = Math.Sqrt(Math.Max(varianceSum / (i + 1), 0));

			//left-edge X pixel is set to the last print X pixel
			pointL.X = pointR.X;
			pointR.X = Chart.GetXCoordinateByBarIndex(barIndex);
			foreach (var id in Enum.GetValues<VWAPIds>())
			{
				pointL.Y = _priorUpperY[id];
				pointR.Y = ChartScale.GetYCoordinateByValue(curVWAP + deviation * _bandSettingsDict[id].Multiplier);
				_priorUpperY[id] = pointR.Y;
				context.DrawLine(pointL, pointR, _bandSettingsDict[id].Color, _bandSettingsDict[id].Thickness, _bandSettingsDict[id].LineStyle);

				//Draw the lower line plot only if this is band 1, 2 or 3
				if (id == VWAPIds.VWAP)
				{
					continue;
				}

				pointL.Y = _priorLowerY[id];
				pointR.Y = ChartScale.GetYCoordinateByValue(curVWAP - deviation * _bandSettingsDict[id].Multiplier);
				_priorLowerY[id] = pointR.Y;
				context.DrawLine(pointL, pointR, _bandSettingsDict[id].Color, _bandSettingsDict[id].Thickness, _bandSettingsDict[id].LineStyle);
			}
		}
	}

	private enum VWAPIds
	{
		VWAP,
		Band1,
		Band2,
		Band3
	}

	private class BandSettings
	{
		public double Multiplier { get; set; }
		public Color Color { get; set; }
		public int Thickness { get; set; } = 1;
		public LineStyle LineStyle { get; set; } = LineStyle.Solid;
	}
}