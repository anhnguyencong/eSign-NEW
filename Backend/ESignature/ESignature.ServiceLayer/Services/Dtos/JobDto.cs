using ESignature.Core.Infrastructure.Collections;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ESignature.DAL.Models
{
    public class JobDto
    {
        public string Id { get; set; }
        public string RefId { get; set; }
        public string PendingFileName { get; set; }
        public IList<CompletedFileDto> CompletedFileUrls { get; set; } = new List<CompletedFileDto>();
        public string JsonData { get; set; }
        public string Status { get; set; }

        [JsonIgnore]
        public string AppCode { get; set; }
        [JsonIgnore]
        public string CompletedFileName { get; set; }
    }

    public class CompletedFileDto
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }

    public class JobMonitorItemDto
    {
        public string Id { get; set; }
        public string RefId { get; set; }
        public string BatchId { get; set; }
        public string SourceName { get; set; }
        public string AppTokenKey { get; set; }
        public string DocumentName { get; set; }
        public string RefNumber { get; set; }
        public string Status { get; set; }
        public string CallbackStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Priority { get; set; }
        [JsonIgnore]
        public string AppCode { get; set; }
        public string CompletedFileName { get; set; }
        public string CompletedFileUrl { get; set; }
        public ICollection<Media> Files { get; set; } = new List<Media>();
        public string Note { get; set; }
    }

    public class JobMonitorDto
    {
        public IPagedList<JobMonitorItemDto> Items { get; set; }
        public JobSummaryDto JobSummary { get; set; }
    }

    public class JobSummaryDto
    {
        public long Total { get; set; }
        public long Completed { get; set; }
        public long InProgress { get; set; }
        public long Pending { get; set; }
        public long Failed { get; set; }
        public long CallbackComplete { get; set; }
        public long CallbackPending { get; set; }
        public long CallbackFailed { get; set; }
    }

}