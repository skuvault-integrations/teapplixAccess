using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using Netco.Logging;
using TeapplixAccess.Misc;
using TeapplixAccess.Models;
using TeapplixAccess.Models.File;
using TeapplixAccess.Services;

namespace TeapplixAccess
{
	public sealed class TeapplixService : ITeapplixService
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

		#region Upload
		public IEnumerable< TeapplixInventoryUploadResponse > InventoryUpload( TeapplixUploadConfig config, Stream file )
		{
			IEnumerable< TeapplixInventoryUploadResponse > result = null;
			var request = this._services.CreateServicePostRequest( config.GetServiceUrl( this._credentials ), this.Boundary );

			using( var requestStream = request.GetRequestStream() )
			{
				requestStream.Write( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

				requestStream.Write( this.FormItemBytes, 0, this.FormItemBytes.Length );

				requestStream.Write( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

				requestStream.Write( this.HeaderBytes, 0, this.HeaderBytes.Length );

				file.CopyTo( requestStream );

				requestStream.Write( this.Trailer, 0, this.Trailer.Length );
			}

			ActionPolicies.TeapplixSubmitPolicy.Do( () =>
				{
					result = this._services.GetUploadResult( request );
				} );
			this.CheckTeapplixUploadSuccess( result );

			return result;
		}

		public async Task< IEnumerable< TeapplixInventoryUploadResponse > > InventoryUploadAsync( TeapplixUploadConfig config, Stream stream )
		{
			var request = this._services.CreateServicePostRequest( config.GetServiceUrl( this._credentials ), this.Boundary );

			using( var requestStream = await request.GetRequestStreamAsync() )
			{
				await requestStream.WriteAsync( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

				await requestStream.WriteAsync( this.FormItemBytes, 0, this.FormItemBytes.Length );

				await requestStream.WriteAsync( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

				await requestStream.WriteAsync( this.HeaderBytes, 0, this.HeaderBytes.Length );

				await stream.CopyToAsync( requestStream );

				await requestStream.WriteAsync( this.Trailer, 0, this.Trailer.Length );
			}

			var result = await this._services.GetUploadResultAsync( request );
			this.CheckTeapplixUploadSuccess( result );

			return result;
		}
		#endregion

		#region Report

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

				return this._services.GetParsedOrders( memStream );
			}
		}
		#endregion

		private void CheckTeapplixUploadSuccess( IEnumerable< TeapplixInventoryUploadResponse > uploadResponse )
		{
			foreach( var item in uploadResponse )
			{
				if( item.Status != InventoryUploadStatusEnum.Success )
					this.LogUploadItemResponseError( item );
			}
		}

		private void InitUploadElements()
		{
			this.Boundary = DateTime.Now.Ticks.ToString("x");
			this.BoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + this.Boundary + "\r\n");
			this.Trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + this.Boundary + "--\r\n");
			this.FormItemBytes = TeapplixUploadTemplates.GetFormDataTemplate();
			this.HeaderBytes = TeapplixUploadTemplates.GetHeaderTemplate();
		}

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