using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using Netco.Logging;
using TeapplixAccess.Models;
using TeapplixAccess.Models.File;

namespace TeapplixAccess.Services
{
	internal class WebRequestServices
	{
		private readonly TeapplixCredentials _credentials;

		public WebRequestServices( TeapplixCredentials credentials )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			this._credentials = credentials;
		}

		public WebRequest CreateServiceGetRequest( Uri serviceUrl )
		{
			var serviceRequest = WebRequest.Create( serviceUrl );
			serviceRequest.Method = "GET";
			return serviceRequest;
		}

		public WebRequest CreateServicePostRequest( Uri serviceUrl, string boundary )
		{
			var serviceRequest = ( HttpWebRequest )WebRequest.Create( serviceUrl );
			serviceRequest.ContentType = "multipart/form-data boundary=" + boundary;
			serviceRequest.Method = "POST";
			serviceRequest.KeepAlive = true;
			serviceRequest.Credentials = CredentialCache.DefaultCredentials;
			return serviceRequest;
		}

		public IEnumerable< TeapplixInventoryUploadResponse > GetUploadResult( WebRequest request )
		{
			IEnumerable< TeapplixInventoryUploadResponse > result;
			using( var response = ( HttpWebResponse )request.GetResponse() )
			{
				try
				{
					var stream = response.GetResponseStream();
					var memStream = new MemoryStream();
					if( stream != null )
						stream.CopyTo( memStream, 0x100 );

					this.Log().LogStream( "response", this._credentials.AccountName, memStream );

					var parser = new TeapplixUploadResponseParser();
					result = parser.Parse( memStream );
				}
				catch( WebException ex )
				{
					this.LogUploadHttpError( ex.Status.ToString() );
					throw;
				}
			}
			return result;
		}

		public async Task< IEnumerable< TeapplixInventoryUploadResponse > > GetUploadResultAsync( WebRequest request )
		{
			IEnumerable<TeapplixInventoryUploadResponse> result;
			using( var response = await request.GetResponseAsync() )
			{
				try
				{
					var stream = response.GetResponseStream();
					var memStream = new MemoryStream();
					if( stream != null )
						await stream.CopyToAsync( memStream, 0x100 );

					this.Log().LogStream( "response", this._credentials.AccountName, memStream );

					var parser = new TeapplixUploadResponseParser();
					result = parser.Parse( memStream );
				}
				catch( WebException ex )
				{
					this.LogUploadHttpError( ex.Status.ToString() );
					throw;
				}
			}

			return result;
		}

		public IEnumerable< TeapplixOrder > GetParsedOrders( MemoryStream memoryStream )
		{
			memoryStream.Seek( 0, SeekOrigin.Begin );
			try
			{
				var parser = new TeapplixExportFileParser();
				return parser.Parse( memoryStream );
			}
			catch
			{
				this.LogParseReportError( memoryStream );
				throw;
			}
		}

		#region logging
		private void LogParseReportError( MemoryStream stream )
		{
			var rawStream = new MemoryStream( stream.ToArray() );
			var reader = new StreamReader( rawStream );
			var rawTeapplixExport = reader.ReadToEnd();

			this.Log().Error( "Failed to parse file for account '{0}':\n\r{1}", this._credentials.AccountName, rawTeapplixExport );
		}

		private void LogUploadHttpError( string status )
		{
			this.Log().Error( "Failed to to upload file for account '{0}'. Request status is '{1}'", this._credentials.AccountName, status );
		}
		#endregion
	}
}