using System;

namespace DeltaDNA
{
	public class TransactionBuilder
	{
		private SDK sdk;
	
		internal TransactionBuilder(SDK sdk)
		{
			this.sdk = sdk;
		}
		
		public void BuyVirtualCurrency(
			string transactionName,
			string realCurrencyType,
			int realCurrencyAmount,
			string virtualCurrencyName,
			string virtualCurrencyType,
			int virtualCurrencyAmount,
			string transactionReceipt = null
		)
		{
			var eventParams = new EventBuilder()
				.AddParam("transactionType", "PURCHASE")
				.AddParam("transactionName", transactionName)
				.AddParam("productsSpent", new ProductBuilder()
					.AddRealCurrency(realCurrencyType, realCurrencyAmount))
				.AddParam("productsReceived", new ProductBuilder()
					.AddVirtualCurrency(virtualCurrencyName, virtualCurrencyType, virtualCurrencyAmount))
				.AddParam("transactionReceipt", transactionReceipt);
				
			sdk.TriggerEvent("transaction", eventParams);
		}
	}
}