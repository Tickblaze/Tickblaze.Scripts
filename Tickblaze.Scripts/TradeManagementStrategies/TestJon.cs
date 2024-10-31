using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.TradeManagementStrategies;

public class TestJon : TradeManagementStrategy
{
	[NumericRange(0, int.MaxValue)]
	[Parameter("Stop-loss distance (ticks)", Description = "Enter the distance from your entry in ticks, that you would like to set your stop loss. Entering zero (0) means you have no stop loss.")]
	public int StopLossTicks { get; set; } = 16;

	[Parameter("Position size type")]
	public SizeType PositionSizeType { get; set; }

	[NumericRange(0, double.MaxValue)]
	[Parameter("Position size value")]
	public double PositionSize { get; set; } = 500;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit #1 distance (ticks)", Description = "Enter the distance from your entry, that you would like to set Target #1. Entering zero (0) means you have no Target #1.")]
	public int FirstTakeProfitTicks { get; set; } = 8;

	[NumericRange(0, double.MaxValue)]
	[Parameter("Take-profit #1 size (%)", Description = "Enter how much % of your position you would like to close at Target #1.")]
	public double FirstTakeProfitSizePercent { get; set; } = 70;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit #2 distance (ticks)", Description = "Enter the distance from your entry, that you would like to set Target #2. Entering zero (0) means you have no Target #2.")]
	public int SecondTakeProfitTicks { get; set; } = 24;

	[NumericRange(0, double.MaxValue)]
	[Parameter("Take-profit #2 size (%)", Description = "Enter how much % of your position you would like to close at Target #2.")]
	public double SecondTakeProfitSizePercent { get; set; } = 30;

	[Parameter("Breakeven stop-loss enabled?", Description = "To make the stop loss move automatically once a certain amount of profit has been gained, set this to True.")]
	public bool EnableAutoBreakeven { get; set; } = false;

	[Parameter("Breakeven trigger (ticks)", Description = "Enter the number of ticks of profit to trigger the stop loss to move.")]
	public int BreakevenTriggerTicks { get; set; } = 8;

	[Parameter("Breakeven offset (ticks)", Description = "Enter the number of ticks of profit you want to protect, once the Breakeven Trigger In Ticks has been reached. A positive number will protect that many ticks, a negative number will leave that many ticks at risk, and allow the trade more room to move before closing.")]
	public int BreakevenOffsetTicks { get; set; } = 1;

	public enum SizeType
	{
		Units,
		EquityRisk,
		EquityRiskPercent
	}

	private readonly List<OrderData> _orderData = [];
	private bool _isBreakEvenTriggered;

	public class OrderData
	{
		public IOrder Entry = null;
		public IOrder StopLoss;
		public IOrder ProfitTarget;
	}

	public TestJon()
	{
		Name = "OCO - Ticks (Jon)";
		Description = @"This TMS script calculates the size of your position based on your Stop Loss Distance (in ticks) and your risk (Calculated in terms of Dollar amount, Percentage Risk or Static Units Risk). It can place 1 or 2 take-profit orders (specified in ticks). You also have the option to set an automatic break-even stop loss function.";
	}

	protected override void Initialize()
	{
		Task.Run(async () =>
		{
			while (IsActive)
			{
				await Task.Delay(100);
				if (_orderData.TrueForAll(order => order.Entry.Status is not OrderStatus.Pending) && Position?.Status is not PositionStatus.Open)
					Stop();
			}
		});
	}

	private void TryMoveToBreakEven()
	{
		if (BreakevenTriggerTicks == 0)
		{
			return;
		}

		var price = Bars.Close[^1];
		var ticks = (int)Math.Round((price - Position.EntryPrice) / Symbol.TickSize);
		var stopPrice = (double?)null;

		if (Position.Direction is OrderDirection.Long)
		{
			if (ticks >= BreakevenTriggerTicks)
			{
				stopPrice = Position.EntryPrice + BreakevenOffsetTicks * Symbol.TickSize;
			}
		}
		else
		{
			if (ticks <= -BreakevenTriggerTicks)
			{
				stopPrice = Position.EntryPrice - BreakevenOffsetTicks * Symbol.TickSize;
			}
		}

		if (stopPrice == null)
		{
			return;
		}

		foreach (var group in _orderData)
		{
			ModifyOrder(group.StopLoss, group.StopLoss.Quantity, stopPrice, null);
		}

		Stop();
	}

	protected override void OnEntryOrder(IOrder order)
	{
		CancelOrder(order);

		var direction = order.Direction is OrderDirection.Long ? 1 : -1;
		var action = order.Direction is OrderDirection.Long ? OrderAction.Buy : OrderAction.SellShort;
		var quantity = (double)CalculateQuantity(PositionSizeType, PositionSize, StopLossTicks, RoundingMode.Down);
		var remainingQuantity = quantity;

		for (var tpIdx = 0; tpIdx < 2 && remainingQuantity > 0; tpIdx++)
		{
			var tpTicks = tpIdx == 0 ? FirstTakeProfitTicks : SecondTakeProfitTicks;
			var tpQuantityPercent = tpIdx == 0 ? FirstTakeProfitSizePercent : SecondTakeProfitSizePercent;
			if (tpTicks == 0 || tpQuantityPercent == 0)
				continue;

			var orderGroupQuantity = Math.Min(remainingQuantity, quantity * tpQuantityPercent);
			remainingQuantity -= orderGroupQuantity;

			_orderData.Add(new OrderData
			{
				Entry = order.Type switch
				{
					OrderType.Stop => PlaceStopOrder(action, quantity, order.StopPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.Limit => PlaceLimitOrder(action, quantity, order.LimitPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.StopLimit => PlaceStopLimitOrder(action, quantity, order.StopPrice, order.LimitPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.Market => ExecuteMarketOrder(action, quantity, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					_ => throw new ArgumentOutOfRangeException()
				}
			});

			_orderData[^1].ProfitTarget = SetTakeProfit(_orderData[^1].Entry, order.Price - StopLossTicks * Symbol.TickSize * direction);
			_orderData[^1].StopLoss = SetStopLoss(_orderData[^1].Entry, order.Price + tpTicks * Symbol.TickSize * direction);
		}

		if (BreakevenTriggerTicks == 0)
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
		return Math.Max(Symbol.MinimumVolume, Symbol.NormalizeVolume(risk / (distanceTicks * Symbol.TickValue), roundingMode));
	}

	protected override void OnOrderUpdate(IOrder order)
	{
		TryMoveToBreakEven();
	}

	protected override void OnBarUpdate()
	{
		if (Position?.Status is not PositionStatus.Open)
		{
			return;
		}

		TryMoveToBreakEven();
	}

	protected override void OnShutdown()
	{
		foreach (var group in _orderData)
			foreach (var order in new[] { group.StopLoss, group.Entry, group.ProfitTarget })
			{
				if (order?.Status is OrderStatus.Pending)
				{
					CancelOrder(order);
				}
			}
	}

	protected override void OnDestroy()
	{
	}
}
