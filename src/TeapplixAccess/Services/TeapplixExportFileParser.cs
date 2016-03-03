using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LumenWorks.Framework.IO.Csv;
using TeapplixAccess.Models.File;

namespace TeapplixAccess.Services
{
	public class TeapplixExportFileParser
	{
		public IList< TeapplixOrder > Parse( Stream source )
		{
			var reader = new StreamReader( source );
			var csvReader = new CsvReader( reader, true ) { MissingFieldAction = MissingFieldAction.ReplaceByEmpty, DefaultHeaderName = "empty_header", SupportsMultiline = true };
			var orders = this.FillOrders( csvReader );

			return orders;
		}

		private List< TeapplixOrder > FillOrders( CsvReader csvReader )
		{
			var result = new List< TeapplixOrder >();
			while( csvReader.ReadNextRecord() )
			{
				if( this.IsEmptyLine( csvReader ) )
					continue;

				var order = new TeapplixOrder();
				order.OrderSource = this.GetValue( csvReader, "order_source" );
				order.AccountId = this.GetValue( csvReader, "account" );
				order.TnxId = this.GetValue( csvReader, "txn_id" );
				order.TnxId2 = this.GetValue( csvReader, "txn_id2" );
				order.PaymentType = this.GetValue( csvReader, "payment_type" );
				order.PaymentAuthInfo = this.GetValue( csvReader, "payment_auth_info" );
				order.FirstName = this.GetValue( csvReader, "first_name" );
				order.LastName = this.GetValue( csvReader, "last_name" );
				order.Email = this.GetValue( csvReader, "payer_email" );
				order.Phone = this.GetValue( csvReader, "contact_phone" );
				order.Country = this.GetValue( csvReader, "address_country" );
				order.State = this.GetValue( csvReader, "address_state" );
				order.AddressZip = this.GetValue( csvReader, "address_zip" );
				order.City = this.GetValue( csvReader, "address_city" );
				order.Address1 = this.GetValue( csvReader, "address_street" );
				order.Address2 = this.GetValue( csvReader, "address_street2" );
				order.Total = this.GetDecimal( this.GetValue( csvReader, "total" ) );
				order.Shipping = this.GetDecimal( this.GetValue( csvReader, "shipping" ) );
				order.Tax = this.GetValue( csvReader, "tax" );
				order.Discount = this.GetValue( csvReader, "discount" );
				order.Fee = this.GetValue( csvReader, "fee" );
				order.Carrier = this.GetValue( csvReader, "carrier" );
				order.Class = this.GetValue( csvReader, "method" );
				order.Weight = this.GetValue( csvReader, "weight" );
				order.Tracking = this.GetValue( csvReader, "tracking" );
				order.Postage = this.GetValue( csvReader, "postage" );
				order.PostageAccount = this.GetValue( csvReader, "postage_account" );
				order.QueueId = this.GetValue( csvReader, "queue_id" );
				order.ItemsCount = this.GetInt( this.GetValue( csvReader, "num_order_lines" ), 1 );

				var dateStr = this.GetValue( csvReader, "date" );
				order.Date = string.IsNullOrEmpty( dateStr ) ? DateTime.MinValue : DateTime.Parse( dateStr, CultureInfo.InvariantCulture );

				DateTime shipDate;
				if( DateTime.TryParse( this.GetValue( csvReader, "ship_date" ), out shipDate ) )
					order.ShipDate = shipDate;

				var startIndex = csvReader.GetFieldIndex( "items" );
				var count = csvReader.FieldCount;
				var fields = new string[ count ];
				csvReader.CopyCurrentRecordTo( fields );

				order.Items = new List< TeapplixItem >();
				for( var i = 0; i < order.ItemsCount; i++ )
				{
					var item = this.FillItem( csvReader, startIndex, i );
					order.Items.Add( item );
				}

				result.Add( order );
			}
			return result;
		}

		private TeapplixItem FillItem( CsvReader csvReader, int startIndex, int itemNumber )
		{
			var startColumn = startIndex + 4 * itemNumber;

			var item = new TeapplixItem
			{
				Descrption = csvReader[ startColumn ],
				Quantity = this.GetInt( csvReader[ startColumn + 1 ] ),
				Sku = csvReader[ startColumn + 2 ],
				Subtotal = this.GetDecimal( csvReader[ startColumn + 3 ] )
			};
			return item;
		}

		public int GetInt( string str, int defaultValue = 0 )
		{
			int value;
			return int.TryParse( str, out value ) ? value : defaultValue;
		}

		public decimal GetDecimal( string str, decimal defaultValue = 0m )
		{
			decimal value;
			return decimal.TryParse( str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value ) ? value : defaultValue;
		}

		public string GetValue( CsvReader csvReader, string columnName )
		{
			return csvReader[ columnName ];
		}

		protected bool IsEmptyLine( CsvReader csvReader )
		{
			var fields = new string[ csvReader.FieldCount ];
			csvReader.CopyCurrentRecordTo( fields );
			return fields.All( string.IsNullOrEmpty );
		}
	}
}