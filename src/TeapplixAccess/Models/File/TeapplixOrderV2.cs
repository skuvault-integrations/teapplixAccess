using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TeapplixAccess.Models.File
{
	[ DataContract ]
	public class TeapplixOrderV2Array
	{
		[ DataMember( Name = "Orders" ) ]
		public List< TeapplixOrderV2 > Orders{ get; set; }
	}

	// Info: https://www.teapplix.com/help/?page_id=5866
	[ DataContract ]
	public class TeapplixOrderV2
	{
		[ DataMember( Name = "TxnId" ) ]
		public string TxnId{ get; set; }

		[ DataMember( Name = "StoreType" ) ]
		public string StoreType{ get; set; }

		[ DataMember( Name = "StoreKey" ) ]
		public string StoreKey{ get; set; }

		[ DataMember( Name = "SellerID" ) ]
		public string SellerID{ get; set; }

		[ DataMember( Name = "PaymentStatus" ) ]
		public string PaymentStatus{ get; set; }

		[ DataMember( Name = "To" ) ]
		public TeapplixOrderV2Address To{ get; set; }

		[ DataMember( Name = "OrderTotals" ) ]
		public TeapplixOrderV2Totals OrderTotals{ get; set; }

		[ DataMember( Name = "OrderDetails" ) ]
		public TeapplixOrderV2Details OrderDetails{ get; set; }

		[ DataMember( Name = "OrderItems" ) ]
		public List< TeapplixOrderV2Item > OrderItems{ get; set; }

		[ DataMember( Name = "ShippingDetails" ) ]
		public List< TeapplixOrderV2Shipment > ShippingDetails{ get; set; }

		public TeapplixOrder ToTeapplixOrder()
		{
			var paymentDate = DateTime.MinValue;
			if( !DateTime.TryParse( this.OrderDetails.PaymentDate, out paymentDate ) )
				paymentDate = DateTime.MinValue;

			DateTime? shipDate = null;
			if( this.ShippingDetails != null && this.ShippingDetails.Any() )
			{
				var dtValue = DateTime.MinValue;
				if( DateTime.TryParse( this.ShippingDetails[ 0 ].ShipDate, out dtValue ) )
					shipDate = dtValue;
			}

			return new TeapplixOrder
			{
				TnxId = this.TxnId,
				Date = paymentDate,
				ShipDate = shipDate,
				FirstName = this.OrderDetails.FirstName,
				LastName = this.OrderDetails.LastName,
				Phone = this.To.PhoneNumber,
				Email = this.To.Email,
				Address1 = this.To.Street,
				Address2 = this.To.Street2,
				City = this.To.City,
				State = this.To.State,
				AddressZip = this.To.ZipCode,
				Country = this.To.Country,
				Carrier = string.Empty,
				Class = this.OrderDetails.ShipClass,
				Total = this.OrderItems.Sum( x => x.Amount ) + this.OrderTotals.Shipping + this.OrderTotals.Tax - this.OrderTotals.Discount,
				Items = this.OrderItems.Select( x => new TeapplixItem
				{
					Sku = x.ItemSKU,
					Quantity = x.Quantity,
					Subtotal = x.Amount
				} ).ToList()
			};
		}
	}

	[ DataContract ]
	public class TeapplixOrderV2Address
	{
		[ DataMember( Name = "Name" ) ]
		public string Name{ get; set; }

		[ DataMember( Name = "Company" ) ]
		public string Company{ get; set; }

		[ DataMember( Name = "Street" ) ]
		public string Street{ get; set; }

		[ DataMember( Name = "Street2" ) ]
		public string Street2{ get; set; }

		[ DataMember( Name = "State" ) ]
		public string State{ get; set; }

		[ DataMember( Name = "City" ) ]
		public string City{ get; set; }

		[ DataMember( Name = "ZipCode" ) ]
		public string ZipCode{ get; set; }

		[ DataMember( Name = "Country" ) ]
		public string Country{ get; set; }

		[ DataMember( Name = "CountryCode" ) ]
		public string CountryCode{ get; set; }

		[ DataMember( Name = "PhoneNumber" ) ]
		public string PhoneNumber{ get; set; }

		[ DataMember( Name = "Email" ) ]
		public string Email{ get; set; }
	}

	[ DataContract ]
	public class TeapplixOrderV2Totals
	{
		[ DataMember( Name = "Shipping" ) ]
		public decimal Shipping{ get; set; }

		[ DataMember( Name = "Tax" ) ]
		public decimal Tax{ get; set; }

		[ DataMember( Name = "Handling" ) ]
		public decimal Handling{ get; set; }

		[ DataMember( Name = "Discount" ) ]
		public decimal Discount{ get; set; }

		[ DataMember( Name = "InsuranceType" ) ]
		public string InsuranceType{ get; set; }

		[ DataMember( Name = "Currency" ) ]
		public string Currency{ get; set; }
	}

	[ DataContract ]
	public class TeapplixOrderV2Details
	{
		[ DataMember( Name = "Invoice" ) ]
		public string Invoice{ get; set; }

		[ DataMember( Name = "PaymentDate" ) ]
		public string PaymentDate{ get; set; }

		[ DataMember( Name = "ShipClass" ) ]
		public string ShipClass{ get; set; }

		[ DataMember( Name = "FirstName" ) ]
		public string FirstName{ get; set; }

		[ DataMember( Name = "LastName" ) ]
		public string LastName{ get; set; }
	}

	[ DataContract ]
	public class TeapplixOrderV2Item
	{
		[ DataMember( Name = "ItemSKU" ) ]
		public string ItemSKU{ get; set; }

		[ DataMember( Name = "Quantity" ) ]
		public int Quantity{ get; set; }

		[ DataMember( Name = "Amount" ) ]
		public decimal Amount{ get; set; }
	}

	[ DataContract ]
	public class TeapplixOrderV2Shipment
	{
		[ DataMember( Name = "ShipDate" ) ]
		public string ShipDate{ get; set; }
	}
}