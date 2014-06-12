using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public class ProductParams
	{
		private Dictionary<string, object> realCurrency;
		private List<Dictionary<string, object>> virtualCurrencies;
		private List<Dictionary<string, object>> items;
		
		//private string productName;
		
		public ProductParams(/*string productName*/)
		{
			//this.productName = productName;
		}
		
		public ProductParams AddRealCurrency(string currencyType, int currencyAmount)
		{
			if (this.realCurrency != null)
			{
				throw new InvalidOperationException("A Product may only have one real currency");
			}
			
			this.realCurrency = new Dictionary<string, object>()
			{
				{ "realCurrencyType", currencyType },
				{ "realCurrencyAmount", currencyAmount }
			};

			return this;
		}
		
		public ProductParams AddVirtualCurrency(string currencyName, string currencyType, int currencyAmount)
		{
			if (this.virtualCurrencies == null)
			{
				this.virtualCurrencies = new List<Dictionary<string, object>>();
			}
		
			this.virtualCurrencies.Add(new Dictionary<string, object>()
			{
				{ "virtualCurrency", new Dictionary<string, object>()
					{
						{ "virtualCurrencyName", currencyName },
						{ "virtualCurrencyType", currencyType },
						{ "virtualCurrencyAmount", currencyAmount }		
					}
				}
			});
		
			return this;
		}
		
		public ProductParams AddItem(string itemName, string itemType, int itemAmount)
		{
			if (this.items == null)
			{
				this.items = new List<Dictionary<string, object>>();
			}
		
			this.items.Add(new Dictionary<string, object>()
			                           {
				{ "item", new Dictionary<string, object>()
					{
						{ "itemName", itemName },
						{ "itemType", itemType },
						{ "itemAmount", itemAmount }		
					}
				}
			});
			
			return this;
		}
		
		public Dictionary<string, object> ToDictionary()
		{
			Dictionary<string, object> contents = new Dictionary<string, object>();
			if (this.realCurrency != null)
			{
				contents.Add("realCurrency", this.realCurrency);
			}
			if (this.virtualCurrencies != null)
			{
				contents.Add("virtualCurrencies", this.virtualCurrencies);
			}
			if (this.items != null)
			{
				contents.Add("items", this.items);
			}
		
//			return new Dictionary<string, object>()
//			{
//				{ this.productName, contents }
//			};

			return contents;
		}
	}
}

