namespace TeapplixAccess.Models
{
	public class TeapplixReportSubaction
	{
		public static readonly TeapplixReportSubaction CustomerRunReport = new TeapplixReportSubaction( "CustomerRun" );
		public static readonly TeapplixReportSubaction InventoryReport = new TeapplixReportSubaction( "Inventory+Report" );

		private TeapplixReportSubaction( string subaction )
		{
			this.Subaction = subaction;
		}

		public string Subaction { get; private set; }
	}

	public enum TeapplixUploadSubactionEnum
	{
		Undefined,
		Inventory,
		ShipConfirm
	}
}