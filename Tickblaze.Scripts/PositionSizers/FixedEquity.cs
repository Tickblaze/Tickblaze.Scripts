using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.PositionSizers;

public class FixedEquity : PositionSizer
{
	[Parameter("Equity per trade", Description = "The fixed amount of equity to invest in a single trade, denominated in the account currency.")]
	public double EquityPerTrade { get; set; } = 5000;

	[Parameter("Enable exit sizing", Description = "Use to indicate whether to enable the position sizing script to determine the size of exit orders.")]
	public bool EnableExitSizing { get; set; } = true;

	public FixedEquity()
	{
		Name = "Fixed Equity";
		ShortName = "FE";
		Description = "This position sizing script is used to allocate a fixed amount of equity on each trade.";
	}

	protected override double GetPositionSize(IOrder order)
	{
		if (EnableExitSizing && Position is not null)
		{
			if (Position.Quantity < order.Quantity)
			{
				return order.Quantity;
			}

			return Position.Quantity;
		}

		var price = Bars.Close[^1];
		if (price <= 0)
		{
			return 0;
		}

		var exchangeRate = GetExchangeRate(Symbol.CurrencyCode, Account.BaseCurrencyCode);
		var size = Math.Floor(EquityPerTrade / (exchangeRate * price));

		return size;
	}
}