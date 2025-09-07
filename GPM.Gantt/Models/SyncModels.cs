using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Represents options for data synchronization.
    /// </summary>
    public class SyncOptions
    {
        /// <summary>
        /// Gets or sets whether to perform bidirectional synchronization.
        /// </summary>
        public bool Bidirectional { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the conflict resolution strategy.
        /// </summary>
        public ConflictResolution ConflictResolution { get; set; } = ConflictResolution.ServerWins;
        
        /// <summary>
        /// Gets or sets the sync scope (which data to synchronize).
        /// </summary>
        public SyncScope Scope { get; set; } = SyncScope.All;
        
        /// <summary>
        /// Gets or sets the last synchronization timestamp.
        /// </summary>
        public DateTime? LastSyncTime { get; set; }
        
        /// <summary>
        /// Gets or sets specific project IDs to synchronize (null for all).
        /// </summary>
        public List<Guid>? ProjectIds { get; set; }
        
        /// <summary>
        /// Gets or sets the batch size for synchronization operations.
        /// </summary>
        public int BatchSize { get; set; } = 100;
    }
    
    /// <summary>
    /// Represents the result of a synchronization operation.
    /// </summary>
    public class SyncResult
    {
        /// <summary>
        /// Gets or sets whether the synchronization was successful.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the synchronization completion timestamp.
        /// </summary>
        public DateTime CompletedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the number of records uploaded.
        /// </summary>
        public int RecordsUploaded { get; set; }
        
        /// <summary>
        /// Gets or sets the number of records downloaded.
        /// </summary>
        public int RecordsDownloaded { get; set; }
        
        /// <summary>
        /// Gets or sets the number of conflicts encountered.
        /// </summary>
        public int ConflictsCount { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of conflicts that occurred.
        /// </summary>
        public List<SyncConflict> Conflicts { get; set; } = new List<SyncConflict>();
        
        /// <summary>
        /// Gets or sets any errors that occurred during synchronization.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the duration of the synchronization operation.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
    
    /// <summary>
    /// Represents the result of an upload operation.
    /// </summary>
    public class UploadResult
    {
        /// <summary>
        /// Gets or sets whether the upload was successful.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the number of records uploaded successfully.
        /// </summary>
        public int SuccessCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of records that failed to upload.
        /// </summary>
        public int FailureCount { get; set; }
        
        /// <summary>
        /// Gets or sets any errors that occurred during upload.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the upload completion timestamp.
        /// </summary>
        public DateTime CompletedAt { get; set; }
    }
    
    /// <summary>
    /// Represents a data change for synchronization.
    /// </summary>
    public class DataChange
    {
        /// <summary>
        /// Gets or sets the unique identifier of the changed entity.
        /// </summary>
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the type of entity that was changed.
        /// </summary>
        public EntityType EntityType { get; set; }
        
        /// <summary>
        /// Gets or sets the type of change operation.
        /// </summary>
        public ChangeOperation Operation { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the change occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the serialized data of the changed entity.
        /// </summary>
        public string Data { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the version of the entity before the change.
        /// </summary>
        public int? PreviousVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the version of the entity after the change.
        /// </summary>
        public int CurrentVersion { get; set; }
    }
    
    /// <summary>
    /// Represents a synchronization conflict.
    /// </summary>
    public class SyncConflict
    {
        /// <summary>
        /// Gets or sets the entity identifier that has a conflict.
        /// </summary>
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the type of entity in conflict.
        /// </summary>
        public EntityType EntityType { get; set; }
        
        /// <summary>
        /// Gets or sets the local version of the entity.
        /// </summary>
        public int LocalVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the remote version of the entity.
        /// </summary>
        public int RemoteVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the serialized local data.
        /// </summary>
        public string LocalData { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the serialized remote data.
        /// </summary>
        public string RemoteData { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets how the conflict was resolved.
        /// </summary>
        public ConflictResolution Resolution { get; set; }
    }
    
    /// <summary>
    /// Defines conflict resolution strategies.
    /// </summary>
    public enum ConflictResolution
    {
        ClientWins,
        ServerWins,
        Manual,
        Merge
    }
    
    /// <summary>
    /// Defines synchronization scope options.
    /// </summary>
    public enum SyncScope
    {
        ProjectsOnly,
        TasksOnly,
        All
    }
    
    /// <summary>
    /// Defines the types of entities that can be synchronized.
    /// </summary>
    public enum EntityType
    {
        Project,
        Task,
        TaskDependency
    }
    
    /// <summary>
    /// Defines the types of change operations.
    /// </summary>
    public enum ChangeOperation
    {
        Create,
        Update,
        Delete
    }
}