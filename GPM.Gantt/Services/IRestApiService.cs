using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for REST API communication and synchronization.
    /// </summary>
    public interface IRestApiService
    {
        /// <summary>
        /// Gets the base URL of the REST API.
        /// </summary>
        string BaseUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the API authentication token.
        /// </summary>
        string? AuthToken { get; set; }
        
        /// <summary>
        /// Sets the authentication credentials for API access.
        /// </summary>
        /// <param name="token">The authentication token.</param>
        void SetAuthentication(string token);
        
        /// <summary>
        /// Tests the connection to the REST API.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if connection is successful, false otherwise.</returns>
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Synchronizes local data with the remote REST API.
        /// </summary>
        /// <param name="syncOptions">Synchronization options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The synchronization result.</returns>
        Task<SyncResult> SynchronizeAsync(SyncOptions syncOptions, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Uploads local changes to the REST API.
        /// </summary>
        /// <param name="changes">The changes to upload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The upload result.</returns>
        Task<UploadResult> UploadChangesAsync(IEnumerable<DataChange> changes, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Downloads changes from the REST API.
        /// </summary>
        /// <param name="lastSyncTime">The last synchronization timestamp.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The downloaded changes.</returns>
        Task<IEnumerable<DataChange>> DownloadChangesAsync(DateTime lastSyncTime, CancellationToken cancellationToken = default);
    }
}