using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESignature.DAL.Models
{
    [Table("ES_Jobs")]
    public class Job : BaseEntity
    {
        public string BatchId { get; set; }
        public string RefId { get; set; }
        public string RefNumber { get; set; }
        public string AppName { get; set; }
        public string AppTokenKey { get; set; }
        public string JsonData { get; set; }
        public string CallBackUrl { get; set; }
        public bool NeedSign { get; set; }
        public bool ConvertToPdf { get; set; }
        public string FilePassword { get; set; }
        public JobPriority Priority { get; set; } = JobPriority.P10;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public CallBackStatus? CallBackStatus { get; set; }
        public DateTime? RequestSignatureApiDate { get; set; }
        public DateTime? ResponseSignatureApiDate { get; set; }
        public string Note { get; set; }
        public string SignerId { get; set; }
        public string Description { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string PageSign { get; set; }
        public string VisiblePosition { get; set; }

        //bit 1: đã gửi lên message broker (vào inprogress queue)
        //bit 2: đã callback
        public int? SentToMessageBroker { get; set; }

        [InverseProperty("Job")]
        public ICollection<Media> Files { get; set; } = new List<Media>();
    }
}