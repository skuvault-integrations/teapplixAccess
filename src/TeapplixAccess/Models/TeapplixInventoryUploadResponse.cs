using System;

namespace TeapplixAccess.Models
{
	public sealed class TeapplixInventoryUploadResponse
	{
		public string Sku { get; private set; }
		public InventoryUploadStatusEnum Status { get; private set; }
		public string Message { get; private set; }

		public TeapplixInventoryUploadResponse( string sku, string status, string message )
		{
			this.Sku = sku;
			this.Status = ( InventoryUploadStatusEnum )Enum.Parse( typeof( InventoryUploadStatusEnum ), status );
			this.Message = message;
		}
	}

	public enum InventoryUploadStatusEnum
	{
		Undefined,
		Success,
		NotFound,
		Failure
	}
}