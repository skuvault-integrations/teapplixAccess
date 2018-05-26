using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TeapplixAccess.Models.File
{
	[ DataContract ]
	public class TeapplixUploadItemV2Request
	{
		[ DataMember ]
		public List< TeapplixUploadItemV2 > Quantities{ get; set; }

		public static TeapplixUploadItemV2Request From( List< TeapplixUploadItem > items )
		{
			return new TeapplixUploadItemV2Request
			{
				Quantities = items.Select( TeapplixUploadItemV2.From ).ToList()
			};
		}
	}

	[ DataContract ]
	public class TeapplixUploadItemV2
	{
		[ DataMember( Name = "PostDate" ) ]
		public string PostDate{ get; set; }

		[ DataMember( Name = "PostType" ) ]
		public string PostType{ get; set; }

		[ DataMember( Name = "PostComment" ) ]
		public string PostComment{ get; set; }

		[ DataMember( Name = "ItemName" ) ]
		public string ItemName{ get; set; }

		[ DataMember( Name = "Quantity" ) ]
		public int Quantity{ get; set; }

		[ DataMember( Name = "UnitPrice" ) ]
		public decimal UnitPrice{ get; set; }

		[ DataMember( Name = "Location" ) ]
		public string Location{ get; set; }

		public static TeapplixUploadItemV2 From( TeapplixUploadItem item )
		{
			var quantity = 0;
			int.TryParse( item.Quantity, out quantity );

			var unitPrice = 0m;
			decimal.TryParse( item.UnitPrice, out unitPrice );

			return new TeapplixUploadItemV2
			{
				PostDate = item.PostDate,
				PostType = item.PostType,
				PostComment = item.PostComment,
				ItemName = item.SKU,
				Quantity = quantity,
				UnitPrice = unitPrice,
				Location = item.Location
			};
		}
	}
}