using System.Diagnostics;
using Tickblaze.Scripts.Api.Adapters;
using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.TradeManagementStrategies;

public class OcoTicks : TradeManagementStrategy
{
	[NumericRange(0, int.MaxValue)]
	[Parameter("Stop-loss distance (ticks)", Description = "Enter the distance from your entry in ticks, that you would like to set your stop loss. Entering zero (0) means you have no stop loss.")]
	public int StopLossTicks { get; set; } = 16;

	[Parameter("Position size type", Description = "Defines what your position size is based on.  Units will use a fixed number of shares/contracts etc. EquityRisk uses a fixed $ amount. EquityRiskPercent will use a fixed % of your account equity.")]
	public SizeType PositionSizeType { get; set; } = SizeType.EquityRisk;

	[NumericRange(0, double.MaxValue)]
	[Parameter("Position size value", Description = "The numeric value to use when calculating position size based on the option selected for Position Size Type.")]
	public double PositionSize { get; set; } = 500;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit #1 distance (ticks)", Description = "Enter the distance from your entry, that you would like to set Target #1. Entering zero (0) means you have no Target #1.")]
	public int FirstTakeProfitTicks { get; set; } = 8;

	[NumericRange(0, 100)]
	[Parameter("Take-profit #1 size (%)", Description = "Enter how much % of your position you would like to close at Target #1.")]
	public double FirstTakeProfitSizePercent { get; set; } = 70;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit #2 distance (ticks)", Description = "Enter the distance from your entry, that you would like to set Target #2. Entering zero (0) means you have no Target #2.")]
	public int SecondTakeProfitTicks { get; set; } = 24;

	[NumericRange(0, 100)]
	[Parameter("Take-profit #2 size (%)", Description = "Enter how much % of your position you would like to close at Target #2.")]
	public double SecondTakeProfitSizePercent { get; set; } = 30;

	[Parameter("Breakeven stop-loss enabled?", Description = "To make the stop loss move automatically once a certain amount of profit has been gained, set this to True.")]
	public bool EnableAutoBreakeven { get; set; } = false;

	[Parameter("Breakeven trigger (ticks)", Description = "Enter the number of ticks of profit to trigger the stop loss to move.")]
	public int BreakevenTriggerTicks { get; set; } = 8;

	[Parameter("Breakeven offset (ticks)", Description = "Enter the number of ticks of profit you want to protect, once the Breakeven Trigger In Ticks has been reached. A positive number will protect that many ticks, a negative number will leave that many ticks at risk, and allow the trade more room to move before closing.")]
	public int BreakevenOffsetTicks { get; set; } = 1;

	private int DirectionAsInt { get; set; }

	private bool BreakEvenValid => EnableAutoBreakeven && BreakevenTriggerTicks > 0;

	private bool ShouldStopTradeManagementStrategy
	{
		get
		{
			// Don't stop if we're still waiting for our entry order to be filled.
			if (_orderData.Any(x => x.Entry.Status is OrderStatus.Pending))
			{
				return false;
			}

			// Stop immediately if there is no break even.
			if (!BreakEvenValid)
			{
				return true;
			}

			// If we're waiting for a break even, stop if our position is no longer valid and all of our entry orders were submitted and reached a final state
			if (Position?.Direction != (DirectionAsInt == 1 ? OrderDirection.Long : OrderDirection.Short) && _orderData.Count > 0 && _orderData.TrueForAll(group => group.Entry.Status != OrderStatus.Pending))
			{
				return true;
			}

			return false;
		}
	}

	private readonly List<OrderData> _orderData = [];

	public OcoTicks()
	{
		Name = "OCO - Ticks";
		Description = "This TMS script calculates the size of your position based on your Stop Loss Distance (in ticks) and your risk (Calculated in terms of Dollar amount, Percentage Risk or Static Units Risk). It can place 1 or 2 take-profit orders (specified in ticks). You also have the option to set an automatic break-even stop loss function.";
	}

	private void TryMoveToBreakEven()
	{
		if (ShouldStopTradeManagementStrategy)
		{
			Stop();
			return;
		}

		if (!BreakEvenValid || Position == null)
		{
			return;
		}

		var price = Bars.Close[^1];
		var ticksInProfit = (int)Math.Round((price - Position.EntryPrice) / Symbol.TickSize) * DirectionAsInt;
		if (ticksInProfit < BreakevenTriggerTicks)
		{
			return;
		}

		var stopPrice = Position.EntryPrice + BreakevenOffsetTicks * DirectionAsInt * Symbol.TickSize;
		foreach (var group in _orderData)
		{
			ModifyOrder(group.StopLoss, group.StopLoss.Quantity, stopPrice, null);
		}

		Stop();
	}

	protected override void OnEntryOrder(IOrder order)
	{
		DirectionAsInt = order.Direction is OrderDirection.Long ? 1 : -1;

		var quantity = CalculateQuantity(PositionSizeType, PositionSize, StopLossTicks, RoundingMode.Down);
		if (quantity < Symbol.MinimumVolume)
		{
			// TODO Add alert here when added to API
			CancelOrder(order, "TMS aborted due to max risk settings");
			Alert?.ShowDialog(AlertType.Bad, "TMS aborted due to max risk settings");
			Stop();
			return;
		}

		CancelOrder(order, "Order replaced by TMS", true);

		if (Position is { Direction: var positionDirection } && positionDirection != order.Direction)
		{
			Alert?.ShowDialog(AlertType.Bad, "Can't submit TMS orders opposing current position");
			Stop();
			return;
		}

		var takeProfits = Enumerable.Range(0, 2)
			.Select(i => (Ticks: i == 0 ? FirstTakeProfitTicks : SecondTakeProfitTicks, SizePercent: i == 0 ? FirstTakeProfitSizePercent : SecondTakeProfitSizePercent))
			.Where(x => x.SizePercent != 0 && x.Ticks != 0)
			.ToList();

		if (takeProfits.Count == 2 && takeProfits[0].SizePercent >= 100)
		{
			takeProfits.RemoveAt(1);
		}

		// Split our order into up to 3 (1 per take profit with valid settings, and one more for the remaining quantity if the take profits don't add up to 100%)
		var orderSpecs = new List<OrderSpec>();
		var remainingPercent = (decimal)100;
		for (var i = 0; i < takeProfits.Count + 1 && remainingPercent > 0; i++)
		{
			var orderGroupPercent = i < takeProfits.Count ? Math.Min((decimal)takeProfits[i].SizePercent, remainingPercent) : remainingPercent;
			remainingPercent -= orderGroupPercent;
			orderSpecs.Add(new OrderSpec
			{
				Quantity = quantity * orderGroupPercent / 100,
				TakeProfitTicks = i < takeProfits.Count ? takeProfits[i].Ticks : null
			});
		}

		// Get the quantity we'd have left to allocate if all brackets were rounded down, then round them all down
		var quantityToDistribute = (decimal)0;
		foreach (var spec in orderSpecs)
		{
			var newQuantity = Symbol.NormalizeVolume((double)spec.Quantity, RoundingMode.Down);
			spec.Remainder = spec.Quantity - newQuantity;
			quantityToDistribute += spec.Remainder;
			spec.Quantity = newQuantity;
		}

		// If two take profits, ensure second take profit has at least some quantity if there's anything left
		if (takeProfits.Count == 2 && orderSpecs[1].Quantity == 0 && quantityToDistribute > 0)
		{
			orderSpecs[1].Quantity = Symbol.MinimumVolume;
			quantityToDistribute -= Symbol.MinimumVolume;
		}

		// Allocate min quantity to the first bracket if it was rounded down and there's any remaining to allocate
		if (takeProfits.Count > 0 && orderSpecs[0].Remainder > 0 && quantityToDistribute > 0)
		{
			orderSpecs[0].Quantity += Symbol.MinimumVolume;
			quantityToDistribute -= Symbol.MinimumVolume;
		}

		// Dump the remaining quantity in the last, stop only bracket
		orderSpecs[^1].Quantity += quantityToDistribute;

		// Remove invalid orders (can happen if there was only 1 order worth of risk allotted)
		orderSpecs.RemoveAll(spec => spec.Quantity == 0);

		var action = order.Direction is OrderDirection.Long ? OrderAction.Buy : OrderAction.SellShort;
		foreach (var spec in orderSpecs)
		{
			_orderData.Add(new OrderData
			{
				Entry = order.Type switch
				{
					OrderType.Stop => PlaceStopOrder(action, (double)spec.Quantity, order.StopPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.Limit => PlaceLimitOrder(action, (double)spec.Quantity, order.LimitPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.StopLimit => PlaceStopLimitOrder(action, (double)spec.Quantity, order.StopPrice, order.LimitPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.Market => ExecuteMarketOrder(action, (double)spec.Quantity, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					_ => throw new ArgumentOutOfRangeException()
				}
			});

			if (spec.TakeProfitTicks != null)
			{
				_orderData[^1].ProfitTarget = SetTakeProfit(_orderData[^1].Entry, order.Price + spec.TakeProfitTicks.Value * Symbol.TickSize * DirectionAsInt);
				_orderData[^1].ProfitTargetTicks = spec.TakeProfitTicks.Value;
			}

			_orderData[^1].StopLoss = SetStopLoss(_orderData[^1].Entry, order.Price - StopLossTicks * Symbol.TickSize * DirectionAsInt);
			_orderData[^1].StopLossTicks = StopLossTicks;
		}

		if (ShouldStopTradeManagementStrategy)
		{
			Stop();
		}
	}

	private decimal CalculateQuantity(SizeType sizeType, double size, int distanceTicks, RoundingMode roundingMode)
	{
		if (sizeType is SizeType.Units)
		{
			return Symbol.NormalizeVolume(size, roundingMode);
		}

		var risk = sizeType is SizeType.EquityRisk ? size : Account.BuyingPower * (size / 100);
		return Symbol.NormalizeVolume(risk / (distanceTicks * Symbol.TickValue), roundingMode);
	}

	protected override void OnOrderUpdate(IOrder order)
	{
		var orderData = _orderData.FirstOrDefault(x => x.Entry == order || x.StopLoss == order || x.ProfitTarget == order);
		if (orderData != null)
		{
			var entry = orderData.Entry;
			var direction = entry.Direction is OrderDirection.Long ? 1 : -1;

			if (order == entry)
			{
				if (order.Status is OrderStatus.Cancelled or OrderStatus.Executed)
				{
					_orderData.Remove(orderData);
				}
				else
				{
					if (orderData.StopLoss.Status is OrderStatus.Pending)
					{
						ModifyOrder(orderData.StopLoss, orderData.StopLoss.Quantity, entry.Price - StopLossTicks * Symbol.TickSize * direction, null);
					}

					if (orderData.ProfitTarget.Status is OrderStatus.Pending)
					{
						ModifyOrder(orderData.ProfitTarget, orderData.ProfitTarget.Quantity, null, entry.Price + orderData.ProfitTargetTicks * Symbol.TickSize * direction);
					}
				}
			}
			else if (order.Status is OrderStatus.Pending)
			{
				if (order == orderData.StopLoss)
				{
					orderData.StopLossTicks = (int)Math.Round((entry.Price - order.StopPrice) / Symbol.TickSize * direction);
				}
				else if (order == orderData.ProfitTarget)
				{
					orderData.ProfitTargetTicks = (int)Math.Round((order.LimitPrice - entry.Price) / Symbol.TickSize * direction);
				}
			}
		}

		TryMoveToBreakEven();
	}

	protected override void OnPositionUpdate()
	{
		TryMoveToBreakEven();
	}

	protected override void OnBarUpdate()
	{
		TryMoveToBreakEven();
	}

	public enum SizeType
	{
		[DisplayName("Fixed Size")] Units,

		[DisplayName("$ Risk")] EquityRisk,

		[DisplayName("% Risk")] EquityRiskPercent
	}

	public class OrderData
	{
		public IOrder Entry;
		public IOrder StopLoss;
		public IOrder ProfitTarget;

		public int StopLossTicks { get; set; }
		public int ProfitTargetTicks { get; set; }
	}

	private class OrderSpec
	{
		public decimal Quantity { get; set; }
		public int? TakeProfitTicks { get; init; }
		public decimal Remainder { get; set; }
	}
}