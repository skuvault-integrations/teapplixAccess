using System;
using System.Net;
using CuttingEdge.Conditions;

namespace TeapplixAccess.Models
{
	public class TeapplixUploadConfig : ITeapplixConfig
	{
		public TeapplixUploadSubactionEnum Subaction { get; private set; }

		/// <summary>
		/// (optional) im_chk_clear=1  => Delete all existing quantity data before uploading.
		/// </summary>
		public bool DeleteQuantity { get; private set; }

		/// <summary>
		/// (optional) im_xref_action=create_product_auto => Automatically create the referenced product if not found.
		/// </summary>
		public bool CreateReferenceProduct { get; private set; }

		public TeapplixUploadConfig( TeapplixUploadSubactionEnum subaction, bool deleteQuantity, bool createReferenceProduct )
		{
			Condition.Requires( subaction, "subaction" ).IsGreaterThan( TeapplixUploadSubactionEnum.Undefined );

			this.Subaction = subaction;
			this.DeleteQuantity = deleteQuantity;
			this.CreateReferenceProduct = createReferenceProduct;
		}

		public Uri GetServiceUrl( TeapplixCredentials credentials )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			var uri = new Uri( string.Format( "https://www.teapplix.com/h/{0}/ea/api.php?User={1}&Passwd={2}&Action=Upload&Subaction={3}{4}",
				credentials.AccountName,
				credentials.UserName,
				credentials.Password,
				this.Subaction,
				this.GetOptionalUploadParameters() ) );

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			return uri;
		}

		private string GetOptionalUploadParameters()
		{
			var result = string.Empty;

			if( this.DeleteQuantity )
				result += "&im_chk_clear=1";
			if( this.CreateReferenceProduct )
				result += "&im_xref_action=create_product_auto";

			return result;
		}
	}
}