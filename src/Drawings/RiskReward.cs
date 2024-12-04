﻿
using System.Diagnostics;

namespace Tickblaze.Scripts.Drawings;

public sealed class RiskReward : Drawing
{
	[Parameter("Stop Risk $", Description = "Max amount of risk, in currency units, for position size calculation")]
	public double StopRiskValue { get; set; } = 2000;

	[Parameter("Entry Color", Description = "Color and opacity of the entry line")]
	public Color EntryColor { get; set; } = Color.Gray;

	[Parameter("Stop Color", Description = "Color and opacity of the stoploss line")]
	public Color StopColor { get; set; } = Color.Red;

	[NumericRange(0, 100)]
	[Parameter("Shading Opacity %", Description = "Opacity of the shading between lines")]
	public int ShadingOpacity { get; set; } = 20;

	[Parameter("T1 Enabled", Description = "Enable Target #1 line")]
	public bool TargetEnabled1 { get; set; } = true;

	[Parameter("T1 Ratio", Description = "Ratio of Target 1 distance to the Stoploss distance, E.g. '1.5' means the Target distance to entry is 150% of the stoploss distance to entry")]
	public double TargetRewardRatio1 { get; set; } = 1.5;

	[Parameter("T1 Exit %", Description = "How many contracts to exit at T2, as a percentage of the whole position size")]
	public double TargetPercent1 { get; set; } = 50;

	[Parameter("T1 Color", Description = "Color and opacity of the Target 1 line")]
	public Color TargetColor1 { get; set; } = "#00ff00";

	[Parameter("T2 Enabled", Description = "Enable Target #2 line")]
	public bool TargetEnabled2 { get; set; } = true;

	[Parameter("T2 Ratio", Description = "Ratio of Target 2 distance to the Stoploss distance, E.g. '1.5' means the Target distance to entry is 150% of the stoploss distance to entry")]
	public double TargetRewardRatio2 { get; set; } = 2.5;

	[Parameter("T2 Exit %", Description = "How many contracts to exit at T2, as a percentage of the whole position size")]
	public double TargetPercent2 { get; set; } = 30;

	[Parameter("T2 Color", Description = "Color and opacity of the Target 2 line")]
	public Color TargetColor2 { get; set; } = "#00ff7f";

	[Parameter("T3 Enabled", Description = "Enable Target #3 line")]
	public bool TargetEnabled3 { get; set; } = true;

	[Parameter("T3 Ratio", Description = "Ratio of Target 3 distance to the Stoploss distance, E.g. '1.5' means the Target distance to entry is 150% of the stoploss distance to entry")]
	public double TargetRewardRatio3 { get; set; } = 3.5;

	[Parameter("T3 Exit %", Description = "How many contracts to exit at T3, as a percentage of the whole position size")]
	public double TargetPercent3 { get; set; } = 20;

	[Parameter("T3 Color", Description = "Color and opacity of the Target 3 line")]
	public Color TargetColor3 { get; set; } = "#00ced1";

	[Parameter("Text Font", Description = "Font name and size for the text")]
	public Font TextFont { get; set; } = new("Arial", 12);

	[Parameter("Text Position", Description = "Location of text being printed")]
	public TextPositionType TextPosition { get; set; } = TextPositionType.Left;

	[Parameter("Lines thickness", Description = "Thickness of the lines"), NumericRange(1, 5)]
	public int LineThickness { get; set; } = 1;

	[Parameter("Lines style", Description = "Line style for the lines")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Extend lines left", Description = "Extend the lines to the left-side of the chart")]
	public bool ExtendLinesLeft { get; set; }

	[Parameter("Extend lines right", Description = "Extend the lines to the right-side of the chart")]
	public bool ExtendLinesRight { get; set; }

	[Parameter("Show Price", Description = "Show price on each of the levels")]
	public bool ShowPrice { get; set; } = true;

	[Parameter("Show Ticks", Description = "Display the distance, in ticks, for each level")]
	public bool ShowTicks { get; set; } = true;

	[Parameter("Show Quantity", Description = "Display the quantity entered/exited at each level")]
	public bool ShowQuantity { get; set; } = true;

	[Parameter("Show PnL", Description = "Display the PnL, in currency units, at each level")]
	public bool ShowProfit { get; set; } = true;

	public IChartPoint PointA => Points[0];
	public IChartPoint PointB => Points[1];
	public int Direction => PointA.Y < PointB.Y ? 1 : -1;

	public override int PointsCount => 2;

	public enum TextPositionType
	{
		Left,
		Right,
	}

	private enum PriceLevelType
	{
		Entry,
		StopLoss,
		TakeProfit
	}

	private record Target(double Ratio, double ExitPercent, Color Color);
	private record PriceLevel(double Price, string Text, Color Color, double? ShadingStartPrice = null);

	private float _shadingOpacity;

	public RiskReward()
	{
		Name = "Risk Reward";
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (TargetEnabled1 is false)
		{
			parameters.Remove(nameof(TargetRewardRatio1));
			parameters.Remove(nameof(TargetPercent1));
			parameters.Remove(nameof(TargetColor1));
		}

		if (TargetEnabled2 is false)
		{
			parameters.Remove(nameof(TargetRewardRatio2));
			parameters.Remove(nameof(TargetPercent2));
			parameters.Remove(nameof(TargetColor2));
		}

		if (TargetEnabled3 is false)
		{
			parameters.Remove(nameof(TargetRewardRatio3));
			parameters.Remove(nameof(TargetPercent3));
			parameters.Remove(nameof(TargetColor3));
		}

		return parameters;
	}

	protected override void Initialize()
	{
		_shadingOpacity = ShadingOpacity / 100f;
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		Points[index].Value = Symbol.RoundToTick((double)yDataValue);
	}

	public override void OnRender(IDrawingContext context)
	{
		var entryPrice = (double)PointA.Value;
		var stopPrice = (double)PointB.Value;
		var stopTicks = (int)Math.Round(Math.Round(entryPrice - stopPrice, Symbol.Decimals) / Symbol.TickSize);
		var stopQuantity = Math.Max(Symbol.MinimumVolume, Symbol.NormalizeVolume(StopRiskValue * Direction / (stopTicks * Symbol.TickValue), RoundingMode.Down));
		var stopLoss = stopTicks * Symbol.TickValue * (double)stopQuantity;

		var levels = new List<PriceLevel>()
		{
			new(entryPrice, GetText(PriceLevelType.Entry, stopQuantity, entryPrice, 0,0), EntryColor),
			new(stopPrice, GetText(PriceLevelType.StopLoss, stopQuantity, stopPrice, -stopTicks, -stopLoss), StopColor, entryPrice),
		};

		var remainingQuantity = stopQuantity;
		var targets = new List<Target>();

		if (TargetEnabled1)
		{
			targets.Add(new(TargetRewardRatio1, TargetPercent1, TargetColor1));
		}

		if (TargetEnabled2)
		{
			targets.Add(new(TargetRewardRatio2, TargetPercent2, TargetColor2));
		}

		if (TargetEnabled3)
		{
			targets.Add(new(TargetRewardRatio3, TargetPercent3, TargetColor3));
		}

		var shadingStartPrice = entryPrice;

		for (var i = 0; i < targets.Count; i++)
		{
			var target = targets[i];
			var price = Symbol.RoundToTick(entryPrice + stopTicks * Symbol.TickSize * target.Ratio);
			var ticks = (int)Math.Round(Symbol.RoundToTick(price - entryPrice) / Symbol.TickSize);
			var quantity = Math.Max(0, Symbol.NormalizeVolume((double)stopQuantity * target.ExitPercent / 100.0, RoundingMode.Up));
			if (quantity > remainingQuantity || i == targets.Count - 1)
			{
				quantity = remainingQuantity;
			}

			var profit = (double)quantity * ticks * Symbol.TickValue;

			remainingQuantity -= quantity;
			levels.Add(new(price, GetText(PriceLevelType.TakeProfit, quantity, price, ticks, profit), target.Color, shadingStartPrice));
			shadingStartPrice = price;
		}

		var minimumWidth = levels.Max(x => context.MeasureText(x.Text, TextFont).Width);

		foreach (var level in levels)
		{
			DrawPriceLevel(context, level, minimumWidth);
		}
	}

	private string GetText(PriceLevelType type, decimal quantity, double price, int ticks, double profit)
	{
		if (profit.Equals(0) is false)
		{
			profit *= Direction;
		}

		var textValues = new List<string>();
		var label = type switch
		{
			PriceLevelType.Entry => Direction > 0 ? "Long" : "Short",
			PriceLevelType.StopLoss => "Stop",
			PriceLevelType.TakeProfit => "Target",
			_ => throw new NotImplementedException(),
		};

		textValues.Add(label);

		if (ShowPrice)
		{
			textValues.Add(price.ToString($"F{Symbol.Decimals}"));
		}

		if (ShowQuantity)
		{
			textValues.Add($"Qty: {quantity}");
		}

		if (type is not PriceLevelType.Entry)
		{
			if (ShowTicks)
			{
				textValues.Add($"Ticks: {ticks}");
			}

			if (ShowProfit)
			{
				textValues.Add($"PnL: {profit:F2}");
			}
		}

		return string.Join(" | ", textValues);
	}

	private void DrawPriceLevel(IDrawingContext context, PriceLevel level, double? width = null)
	{
		var y = ChartScale.GetYCoordinateByValue(level.Price);
		var pointA = new Point(Math.Min(PointA.X, PointB.X), y);
		var pointB = new Point(Math.Max(PointA.X, PointB.X), y);

		if (width.HasValue)
		{
			var x = pointA.X + width.Value;
			if (x > pointB.X)
			{
				pointB.X = x;
			}
		}

		var textSize = context.MeasureText(level.Text, TextFont);
		var textOrigin = new Point(TextPosition is TextPositionType.Left ? pointA.X : pointB.X - textSize.Width, y - textSize.Height);

		if (ExtendLinesRight)
		{
			pointB.X = context.RenderSize.Width;

			if (TextPosition is TextPositionType.Right)
			{
				textOrigin.X = pointB.X - textSize.Width - 2;
			}
		}

		if (ExtendLinesLeft)
		{
			pointA.X = 0;

			if (TextPosition is TextPositionType.Left)
			{
				textOrigin.X = 3;
			}
		}

		if (_shadingOpacity > 0 && level.ShadingStartPrice.HasValue)
		{
			var y2 = ChartScale.GetYCoordinateByValue(level.ShadingStartPrice.Value);
			var color = Color.New(level.Color, _shadingOpacity);

			context.DrawRectangle(pointA, new Point(pointB.X, y2), color);
		}

		context.DrawLine(pointA, pointB, level.Color, LineThickness, LineStyle);
		context.DrawText(textOrigin, level.Text, level.Color, TextFont);
	}
}
