using System;

namespace DeltaDNA
{
	public class TransactionBuilder
	{
		private DDNA ddna;
	
		internal TransactionBuilder(DDNA ddna)
		{
			this.ddna = ddna;
		}
		
		public void BuyVirtualCurrency(
			string transactionName,
			string realCurrencyType,
			int realCurrencyAmount,
			string virtualCurrencyName,
			string virtualCurrencyType,
			int virtualCurrencyAmount
		)
		{
			this.BuyVirtualCurrency(
				transactionName, 
				realCurrencyType, 
				realCurrencyAmount, 
				virtualCurrencyName, 
				virtualCurrencyType, 
				virtualCurrencyAmount, 
				null);
		}
		
		public void BuyVirtualCurrency(
			string transactionName,
			string realCurrencyType,
			int realCurrencyAmount,
			string virtualCurrencyName,
			string virtualCurrencyType,
			int virtualCurrencyAmount,
			string transactionReceipt
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
				
			ddna.RecordEvent("transaction", eventParams);
		}
	}
}