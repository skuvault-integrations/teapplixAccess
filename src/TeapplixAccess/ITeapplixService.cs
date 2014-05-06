using System.Collections.Generic;
using System.Threading.Tasks;
using TeapplixAccess.Models;
using TeapplixAccess.Models.File;

namespace TeapplixAccess
{
	public interface ITeapplixService
	{
		/// <summary>
		/// upload inventory file
		/// http://www.teapplix.com/help/wp-content/uploads/2013/02/upload.csv
		/// </summary>
		/// <param name="uploadItems">items to upload</param>
		/// <returns>Information about the not added/updated items</returns>
		IEnumerable< TeapplixInventoryUploadResponse > InventoryUpload( IEnumerable< TeapplixUploadItem > uploadItems );

		/// <summary>
		/// upload inventory file async
		/// http://www.teapplix.com/help/wp-content/uploads/2013/02/upload.csv
		/// </summary>
		/// <param name="uploadItems">items to upload</param>
		/// <returns>Information about the not added/updated items</returns>
		Task< IEnumerable< TeapplixInventoryUploadResponse > > InventoryUploadAsync( IEnumerable< TeapplixUploadItem > uploadItems );

		/// <summary>
		/// Download customer report
		///  http://www.teapplix.com/help/wp-content/uploads/2011/05/GenericCSVImport.csv
		/// </summary>
		/// <returns>parsed report</returns>
		IEnumerable< TeapplixOrder > GetCustomerReport( TeapplixReportConfig config );

		/// <summary>
		/// Download customer report async
		/// http://www.teapplix.com/help/wp-content/uploads/2011/05/GenericCSVImport.csv
		/// </summary>
		/// <returns>parsed report</returns>
		Task< IEnumerable< TeapplixOrder > > GetCustomerReportAsync( TeapplixReportConfig config );
	}
}