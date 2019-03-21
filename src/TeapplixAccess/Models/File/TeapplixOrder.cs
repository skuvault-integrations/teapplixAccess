using System;
using System.Collections.Generic;

namespace TeapplixAccess.Models.File
{
	public sealed class TeapplixOrder: IEquatable< TeapplixOrder >
	{
		public string OrderSource;
		public string AccountId;
		public string TnxId;
		public string TnxId2;
		public DateTime Date;
		public string PaymentStatus;
		public string PaymentType;
		public string PaymentAuthInfo;
		public string FirstName;
		public string LastName;
		public string Email;
		public string Phone;
		public string Country;
		public string State;
		public string AddressZip;
		public string City;
		public string Address1;
		public string Address2;
		public string Currency;
		public decimal Total;
		public decimal? Shipping;
		public string Tax;
		public string Discount;
		public string Fee;
		public DateTime? ShipDate;
		public string Carrier;
		public string Class;
		public string Weight;
		public string Tracking;
		public string Postage;
		public string PostageAccount;
		public string QueueId;
		public string Insurance;
		public string InsuranceValue;
		public string InsuranceFee;
		public int ItemsCount;
		public List< TeapplixItem > Items;

		public bool IsShipped
		{
			get { return ShipDate.HasValue; }
		}

		public bool Equals( TeapplixOrder other )
		{
			if( ReferenceEquals( null, other ) )
				return false;
			if( ReferenceEquals( this, other ) )
				return true;
			return string.Equals( this.TnxId, other.TnxId ) && string.Equals( this.AccountId, other.AccountId ) && string.Equals( this.OrderSource, other.OrderSource );
		}

		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) )
				return false;
			if( ReferenceEquals( this, obj ) )
				return true;
			if( obj.GetType() != this.GetType() )
				return false;
			return this.Equals( ( TeapplixOrder )obj );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = ( this.TnxId != null ? this.TnxId.GetHashCode() : 0 );
				hashCode = ( hashCode * 397 ) ^ ( this.AccountId != null ? this.AccountId.GetHashCode() : 0 );
				hashCode = ( hashCode * 397 ) ^ ( this.OrderSource != null ? this.OrderSource.GetHashCode() : 0 );
				return hashCode;
			}
		}

		public static bool operator ==( TeapplixOrder left, TeapplixOrder right )
		{
			return Equals( left, right );
		}

		public static bool operator !=( TeapplixOrder left, TeapplixOrder right )
		{
			return !Equals( left, right );
		}
	}
}