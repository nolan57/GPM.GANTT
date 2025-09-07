using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Implementation of REST API service for GPM Gantt integration.
    /// </summary>
    public class RestApiService : IRestApiService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed = false;

        /// <summary>
        /// Gets or sets the base URL of the REST API.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the API authentication token.
        /// </summary>
        public string? AuthToken { get; set; }

        /// <summary>
        /// Initializes a new instance of the RestApiService class.
        /// </summary>
        /// <param name="baseUrl">The base URL of the REST API.</param>
        public RestApiService(string baseUrl)
        {
            BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Sets the authentication credentials for API access.
        /// </summary>
        /// <param name="token">The authentication token.</param>
        public void SetAuthentication(string token)
        {
            AuthToken = token ?? throw new ArgumentNullException(nameof(token));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Tests the connection to the REST API.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if connection is successful, false otherwise.</returns>
        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/health", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Synchronizes local data with the remote REST API.
        /// </summary>
        /// <param name="syncOptions">Synchronization options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The synchronization result.</returns>
        public async Task<SyncResult> SynchronizeAsync(SyncOptions syncOptions, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var result = new SyncResult
            {
                CompletedAt = DateTime.UtcNow
            };

            try
            {
                // Download remote changes first
                var remoteChanges = await DownloadChangesAsync(syncOptions.LastSyncTime ?? DateTime.MinValue, cancellationToken);
                var remoteChangesList = remoteChanges.ToList();
                result.RecordsDownloaded = remoteChangesList.Count;

                // Apply remote changes locally (this would integrate with local storage)
                await ApplyRemoteChangesAsync(remoteChangesList, syncOptions.ConflictResolution, result, cancellationToken);

                // Upload local changes if bidirectional sync is enabled
                if (syncOptions.Bidirectional)
                {
                    var localChanges = await GetLocalChangesAsync(syncOptions.LastSyncTime ?? DateTime.MinValue, syncOptions.ProjectIds, cancellationToken);
                    var uploadResult = await UploadChangesAsync(localChanges, cancellationToken);
                    result.RecordsUploaded = uploadResult.SuccessCount;
                    result.Errors.AddRange(uploadResult.Errors);
                }

                result.Success = result.Errors.Count == 0;
                result.Duration = DateTime.UtcNow - startTime;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Synchronization failed: {ex.Message}");
                result.Duration = DateTime.UtcNow - startTime;
            }

            return result;
        }

        /// <summary>
        /// Uploads local changes to the REST API.
        /// </summary>
        /// <param name="changes">The changes to upload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The upload result.</returns>
        public async Task<UploadResult> UploadChangesAsync(IEnumerable<DataChange> changes, CancellationToken cancellationToken = default)
        {
            var result = new UploadResult
            {
                CompletedAt = DateTime.UtcNow
            };

            try
            {
                var changesList = changes.ToList();
                var batches = CreateBatches(changesList, 100); // Process in batches of 100

                foreach (var batch in batches)
                {
                    var json = JsonSerializer.Serialize(batch, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("api/sync/upload", content, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        result.SuccessCount += batch.Count;
                    }
                    else
                    {
                        result.FailureCount += batch.Count;
                        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        result.Errors.Add($"Failed to upload batch: {response.StatusCode} - {errorContent}");
                    }
                }

                result.Success = result.FailureCount == 0;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Upload failed: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Downloads changes from the REST API.
        /// </summary>
        /// <param name="lastSyncTime">The last synchronization timestamp.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The downloaded changes.</returns>
        public async Task<IEnumerable<DataChange>> DownloadChangesAsync(DateTime lastSyncTime, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"api/sync/changes?since={lastSyncTime:yyyy-MM-ddTHH:mm:ssZ}";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<List<DataChange>>(json, _jsonOptions) ?? new List<DataChange>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to download changes: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error downloading changes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Applies remote changes to local storage.
        /// </summary>
        private async Task ApplyRemoteChangesAsync(IEnumerable<DataChange> changes, ConflictResolution conflictResolution, SyncResult result, CancellationToken cancellationToken)
        {
            foreach (var change in changes)
            {
                try
                {
                    // This would integrate with your local storage/database
                    // For now, we'll just simulate processing
                    await Task.Delay(1, cancellationToken);
                    
                    // Handle conflicts based on the resolution strategy
                    if (await HasConflictAsync(change, cancellationToken))
                    {
                        var conflict = await ResolveConflictAsync(change, conflictResolution, cancellationToken);
                        result.Conflicts.Add(conflict);
                        result.ConflictsCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to apply change for {change.EntityType} {change.EntityId}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gets local changes since the specified timestamp.
        /// </summary>
        private async Task<IEnumerable<DataChange>> GetLocalChangesAsync(DateTime since, List<Guid>? projectIds, CancellationToken cancellationToken)
        {
            // This would integrate with your local change tracking
            // For now, return empty collection
            await Task.Delay(1, cancellationToken);
            return new List<DataChange>();
        }

        /// <summary>
        /// Checks if a change conflicts with local data.
        /// </summary>
        private async Task<bool> HasConflictAsync(DataChange change, CancellationToken cancellationToken)
        {
            // This would check against local storage for version conflicts
            await Task.Delay(1, cancellationToken);
            return false; // Simplified implementation
        }

        /// <summary>
        /// Resolves a synchronization conflict.
        /// </summary>
        private async Task<SyncConflict> ResolveConflictAsync(DataChange change, ConflictResolution resolution, CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            
            return new SyncConflict
            {
                EntityId = change.EntityId,
                EntityType = change.EntityType,
                LocalVersion = change.PreviousVersion ?? 0,
                RemoteVersion = change.CurrentVersion,
                LocalData = string.Empty, // Would be populated from local storage
                RemoteData = change.Data,
                Resolution = resolution
            };
        }

        /// <summary>
        /// Creates batches from a collection of changes.
        /// </summary>
        private static IEnumerable<List<T>> CreateBatches<T>(List<T> items, int batchSize)
        {
            for (int i = 0; i < items.Count; i += batchSize)
            {
                yield return items.Skip(i).Take(batchSize).ToList();
            }
        }

        /// <summary>
        /// Disposes the HTTP client resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}