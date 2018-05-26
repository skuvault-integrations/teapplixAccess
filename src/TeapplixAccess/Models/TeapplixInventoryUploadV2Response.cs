using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TeapplixAccess.Models
{
	[ DataContract ]
	public class TeapplixInventoryUploadV2Response
	{
		[ DataMember( Name = "Products" ) ]
		public List< TeapplixInventoryUploadV2ResponseItem > Products{ get; set; }

		public List< TeapplixInventoryUploadResponse > To()
		{
			if( this.Products == null )
				return new List< TeapplixInventoryUploadResponse >();

			return this.Products.Select( x => x.To() ).ToList();
		}
	}

	[ DataContract ]
	public class TeapplixInventoryUploadV2ResponseItem
	{
		[ DataMember( Name = "ItemName" ) ]
		public string ItemName{ get; set; }

		[ DataMember( Name = "Status" ) ]
		public string Status{ get; set; }

		[ DataMember( Name = "Message" ) ]
		public string Message{ get; set; }

		public TeapplixInventoryUploadResponse To()
		{
			return new TeapplixInventoryUploadResponse( this.ItemName, this.Status, this.Message );
		}
	}
}