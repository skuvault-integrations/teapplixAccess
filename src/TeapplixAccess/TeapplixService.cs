using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using LINQtoCSV;
using Netco.Logging;
using TeapplixAccess.Misc;
using TeapplixAccess.Models;
using TeapplixAccess.Models.File;
using TeapplixAccess.Services;

namespace TeapplixAccess
{
	public sealed class TeapplixService: ITeapplixService
	{
		private readonly WebRequestServices _services;
		private readonly TeapplixCredentials _credentials;
		private string Boundary;
		private byte[] BoundaryBytes;
		private byte[] Trailer;
		private byte[] FormItemBytes;
		private byte[] HeaderBytes;

		public TeapplixService( TeapplixCredentials credentials )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			this._services = new WebRequestServices( credentials );
			this._credentials = credentials;
			this.InitUploadElements();
		}

		#region Upload Inventory
		public IEnumerable< TeapplixInventoryUploadResponse > InventoryUpload( IEnumerable< TeapplixUploadItem > uploadItems )
		{
			IEnumerable< TeapplixInventoryUploadResponse > uploadResult;

			using( var ms = new MemoryStream() )
			using( var writer = new StreamWriter( ms ) )
			{
				this.FillMemoryStream( ms, writer, uploadItems );
				uploadResult = this.Upload( new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false ), ms );
			}

			return uploadResult;
		}

		public async Task< IEnumerable< TeapplixInventoryUploadResponse > > InventoryUploadAsync( IEnumerable< TeapplixUploadItem > uploadItems )
		{
			IEnumerable< TeapplixInventoryUploadResponse > uploadResult;

			using( var ms = new MemoryStream() )
			using( var writer = new StreamWriter( ms ) )
			{
				this.FillMemoryStream( ms, writer, uploadItems );
				uploadResult = await this.UploadAsync( new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false ), ms );
			}

			return uploadResult;
		}

		private async Task< IEnumerable< TeapplixInventoryUploadResponse > > UploadAsync( TeapplixUploadConfig config, Stream file )
		{
			var request = this._services.CreateServicePostRequest( config.GetServiceUrl( this._credentials ), this.Boundary );

			using( var requestStream = new LoggingStream( await request.GetRequestStreamAsync() ) )
			{
				await requestStream.WriteAsync( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

				await requestStream.WriteAsync( this.FormItemBytes, 0, this.FormItemBytes.Length );

				await requestStream.WriteAsync( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

				await requestStream.WriteAsync( this.HeaderBytes, 0, this.HeaderBytes.Length );

				await file.CopyToAsync( requestStream );

				await requestStream.WriteAsync( this.Trailer, 0, this.Trailer.Length );

				this.Log().LogStream( "request", this._credentials.AccountName, requestStream );
			}

			var result = await this._services.GetUploadResultAsync( request );
			this.CheckTeapplixUploadSuccess( result );

			return result;
		}

		private IEnumerable< TeapplixInventoryUploadResponse > Upload( ITeapplixConfig config, Stream file )
		{
			IEnumerable< TeapplixInventoryUploadResponse > result = null;
			var request = this._services.CreateServicePostRequest( config.GetServiceUrl( this._credentials ), this.Boundary );

			using( var requestStream = new LoggingStream( request.GetRequestStream() ) )
			{
				requestStream.Write( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

				requestStream.Write( this.FormItemBytes, 0, this.FormItemBytes.Length );

				requestStream.Write( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

				requestStream.Write( this.HeaderBytes, 0, this.HeaderBytes.Length );

				file.CopyTo( requestStream );

				requestStream.Write( this.Trailer, 0, this.Trailer.Length );

				this.Log().LogStream( "request", this._credentials.AccountName, requestStream );
			}

			ActionPolicies.TeapplixSubmitPolicy.Do( () =>
			{
				result = this._services.GetUploadResult( request );
			} );
			this.CheckTeapplixUploadSuccess( result );

			return result;
		}
		#endregion

		#region Customer Report (Orders)
		public IEnumerable< TeapplixOrder > GetCustomerReport( TeapplixReportConfig config )
		{
			var reequest = this._services.CreateServiceGetRequest( config.GetServiceUrl( this._credentials ) );

			using( var response = reequest.GetResponse() )
			using( var responseStream = response.GetResponseStream() )
			{
				if( responseStream == null )
				{
					this.LogReportResponseError();
					return Enumerable.Empty< TeapplixOrder >();
				}

				var memStream = new MemoryStream();
				responseStream.CopyTo( memStream, 0x1000 );

				this.Log().LogStream( "response", this._credentials.AccountName, memStream );

				var orders = ActionPolicies.TeapplixGetPolicy.Get( () => this._services.GetParsedOrders( memStream ) );
				return orders;
			}
		}

		public async Task< IEnumerable< TeapplixOrder > > GetCustomerReportAsync( TeapplixReportConfig config )
		{
			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;
			var reequest = this._services.CreateServiceGetRequest( config.GetServiceUrl( this._credentials ) );

			using( var response = await reequest.GetResponseAsync() )
			using( var responseStream = response.GetResponseStream() )
			{
				if( responseStream == null )
				{
					this.LogReportResponseError();
					return Enumerable.Empty< TeapplixOrder >();
				}

				var memStream = new MemoryStream();
				await responseStream.CopyToAsync( memStream, 0x1000, token );

				this.Log().LogStream( "response", this._credentials.AccountName, memStream );

				return this._services.GetParsedOrders( memStream );
			}
		}
		#endregion

		#region Misc
		private void CheckTeapplixUploadSuccess( IEnumerable< TeapplixInventoryUploadResponse > uploadResponse )
		{
			foreach( var item in uploadResponse.Where( item => item.Status != InventoryUploadStatusEnum.Success ) )
			{
				this.LogUploadItemResponseError( item );
			}
		}

		private void InitUploadElements()
		{
			this.Boundary = DateTime.Now.Ticks.ToString( "x" );
			this.BoundaryBytes = Encoding.ASCII.GetBytes( "\r\n--" + this.Boundary + "\r\n" );
			this.Trailer = Encoding.ASCII.GetBytes( "\r\n--" + this.Boundary + "--\r\n" );
			this.FormItemBytes = TeapplixUploadTemplates.GetFormDataTemplate();
			this.HeaderBytes = TeapplixUploadTemplates.GetHeaderTemplate();
		}

		private CsvFileDescription CreateCsvFileDescription()
		{
			return new CsvFileDescription
			{
				SeparatorChar = ',',
				FirstLineHasColumnNames = true,
			};
		}

		private void FillMemoryStream( Stream memoryStream, TextWriter streamWriter, IEnumerable< TeapplixUploadItem > uploadItems )
		{
			var context = new CsvContext();
			context.Write( uploadItems, streamWriter, this.CreateCsvFileDescription() );
			streamWriter.Flush();
			memoryStream.Position = 0;
		}
		#endregion

		#region Logging
		public void LogReportResponseError()
		{
			this.Log().Error( "Failed to get file for account '{0}'", this._credentials.AccountName );
		}

		public void LogUploadItemResponseError( TeapplixInventoryUploadResponse response )
		{
			this.Log().Error( "Failed to upload item with SKU '{0}'. Status code:'{1}', message: {2}", response.Sku, response.Status, response.Message );
		}
		#endregion
	}
}