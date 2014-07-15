namespace TeapplixAccess.Models
{
	public class TeapplixUploadQuantityPostType
	{
		/// <summary>
		/// this is a count that you provide after end of the shipping date. For any product, an “in-stock” makes 
		/// Teapplix ignore any quantity entries before this date, and start counting inventory from 
		/// after this date onwards.
		/// </summary>
		public static readonly TeapplixUploadQuantityPostType InStock = new TeapplixUploadQuantityPostType( "in-stock" );

		/// <summary>
		/// this corresponds to you purchasing and receiving additional quantities of certain inventory. 
		/// Note that the subtotal field allows Teapplix to calculate the latest cost of your items and is
		///  used in the profit and loss calculations.
		/// </summary>
		public static readonly TeapplixUploadQuantityPostType Credit = new TeapplixUploadQuantityPostType( "credit" );

		/// <summary>
		/// this is used to track loss / damage of inventory items, moving inventory out of the count 
		/// that is not reflected by an “Order” that is in your Teapplix account.
		/// </summary>
		public static readonly TeapplixUploadQuantityPostType Debit = new TeapplixUploadQuantityPostType( "debit" );

		private TeapplixUploadQuantityPostType( string type )
		{
			this.PostType = type;
		}

		public string PostType { get; private set; }
	}
}