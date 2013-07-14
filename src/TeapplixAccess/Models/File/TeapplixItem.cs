namespace TeapplixAccess.Models.File
{
	public sealed class TeapplixItem
	{
		public string Descrption { get; set; }
		public int Quantity { get; set; }
		public string Sku { get; set; }
		public decimal Subtotal { get; set; }

		public decimal UnitPrice
		{
			get { return this.Quantity != 0 ? this.Subtotal / this.Quantity : 0m; }
		}
	}
}