using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CuttingEdge.Conditions;
using LINQtoCSV;
using TeapplixAccess.Misc;
using TeapplixAccess.Models;
using TeapplixAccess.Models.File;
using TeapplixAccess.Services;

namespace TeapplixAccess
{
	public sealed class TeapplixService: ITeapplixService
	{
		private readonly bool _useV2;
		private readonly WebRequestServices _services;
		private readonly HttpClient _httpClient;

		private readonly TeapplixCredentials _credentials;
		private string Boundary;
		private byte[] BoundaryBytes;
		private byte[] Trailer;
		private byte[] FormItemBytes;
		private byte[] HeaderBytes;

		public TeapplixService( TeapplixCredentials credentials )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			if( string.IsNullOrEmpty( credentials.ApiKey ) )
			{
				this._useV2 = false;
				this._services = new WebRequestServices( credentials );
				this._credentials = credentials;
				this.InitUploadElements();
			}
			else
			{
				this._useV2 = true;
				this._credentials = credentials;
				this._httpClient = new HttpClient();
				this._httpClient.DefaultRequestHeaders.Add( "APIToken", this._credentials.ApiKey );
			}
		}

		#region Upload Inventory
		public IEnumerable< TeapplixInventoryUploadResponse > InventoryUpload( IEnumerable< TeapplixUploadItem > uploadItems )
		{
			if( this._useV2 )
				return this.InventoryUploadV2Async( uploadItems ).Result;

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
			if( this._useV2 )
				return await this.InventoryUploadV2Async( uploadItems );

			IEnumerable< TeapplixInventoryUploadResponse > uploadResult;

			using( var ms = new MemoryStream() )
			using( var writer = new StreamWriter( ms ) )
			{
				this.FillMemoryStream( ms, writer, uploadItems );
				uploadResult = await this.UploadAsync( new TeapplixUploadConfig( TeapplixUploadSubactionEnum.Inventory, false, false ), ms );
			}

			return uploadResult;
		}

		private async Task< IEnumerable< TeapplixInventoryUploadResponse > > InventoryUploadV2Async( IEnumerable< TeapplixUploadItem > uploadItems )
		{
			var requestUri = new Uri( "https://api.teapplix.com/api2/ProductQuantity" );
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			var jss = new JavaScriptSerializer();
			var jsonContent = jss.Serialize( TeapplixUploadItemV2Request.From( uploadItems.ToList() ) );
			var content = new StringContent( jsonContent, Encoding.UTF8, "application/json" );

			var response = await this._httpClient.PostAsync( requestUri, content );
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			var responseObj = jss.Deserialize< TeapplixInventoryUploadV2Response >( json );
			var responseAsV1 = responseObj.To();

			return responseAsV1;
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

				LogServices.Logger.LogStream( "request", this._credentials.AccountName, requestStream );
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

				LogServices.Logger.LogStream( "request", this._credentials.AccountName, requestStream );
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
			if( this._useV2 )
				return this.GetCustomerReportV2Async( config ).Result;

			var reequest = this._services.CreateServiceGetRequest( config.GetServiceUrl( this._credentials ) );
			
			using( var response = reequest.GetResponse() )
			using( var responseStream = response.GetResponseStream() )
			{
				if( responseStream == null )
				{
					this.LogReportResponseError();
					return Enumerable.Empty< TeapplixOrder >();
				}

				IEnumerable< TeapplixOrder > orders;
				using( var memStream = new MemoryStream() )
				{
					responseStream.CopyTo( memStream, 0x1000 );

					LogServices.Logger.LogStream( "response", this._credentials.AccountName, memStream );

					orders = this._services.GetParsedOrders( memStream );
				}
				return orders;
			}
		}

		public async Task< IEnumerable< TeapplixOrder > > GetCustomerReportAsync( TeapplixReportConfig config )
		{
			if( this._useV2 )
				return this.GetCustomerReportV2Async( config ).Result;

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

				using( var memStream = new MemoryStream() )
				{
					await responseStream.CopyToAsync( memStream, 0x1000, token );

					LogServices.Logger.LogStream( "response", this._credentials.AccountName, memStream );

					return this._services.GetParsedOrders( memStream );
				}
			}
		}

		private async Task< IEnumerable< TeapplixOrder > > GetCustomerReportV2Async( TeapplixReportConfig config )
		{
			var requestUri = config.GetUrlForGetOrdersV2();

			var response = await this._httpClient.GetAsync( requestUri );
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			var jss = new JavaScriptSerializer();
			var orders = jss.Deserialize< TeapplixOrderV2Array >( json );
			var ordersAsV1 = orders.Orders.Select( x => x.ToTeapplixOrder() ).ToList();

			return ordersAsV1;
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
			LogServices.Logger.Error( "Failed to get file for account '{0}'", this._credentials.AccountName );
		}

		public void LogUploadItemResponseError( TeapplixInventoryUploadResponse response )
		{
			LogServices.Logger.Error( "Failed to upload item with SKU '{0}'. Status code:'{1}', message: {2}", response.Sku, response.Status, response.Message );
		}
		#endregion
	}
}