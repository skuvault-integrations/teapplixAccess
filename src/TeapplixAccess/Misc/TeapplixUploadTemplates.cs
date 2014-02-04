using System.Collections.Specialized;

namespace TeapplixAccess.Misc
{
	internal static class TeapplixUploadTemplates
	{
		public static byte[] GetFormDataTemplate()
		{
			const string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			var nvc = new NameValueCollection { { "btn-submit", "Upload" } };
			var nvcKey = nvc.Keys[ 0 ];
			var formitem = string.Format( formdataTemplate, nvcKey, nvc[ nvcKey ] );

			return System.Text.Encoding.UTF8.GetBytes( formitem );
		}

		public static byte[] GetHeaderTemplate()
		{
			const string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
			var header = string.Format( headerTemplate, "upload", "upload.txt", "text/plain" );

			return System.Text.Encoding.UTF8.GetBytes( header );
		}
	}
}