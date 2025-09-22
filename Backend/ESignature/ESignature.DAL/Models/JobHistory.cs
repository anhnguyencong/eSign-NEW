using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESignature.DAL.Models
{
    [Table("ES_JobHistories")]
    public class JobHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string BatchId { get; set; }
        public string RefId { get; set; }
        public string AppName { get; set; }
        public string AppCode { get; set; }
        public string AppTokenKey { get; set; }
        public string JsonData { get; set; }
        public string CallBackUrl { get; set; }
        public bool NeedSign { get; set; }
        public bool ConvertToPdf { get; set; }
        public string FilePassword { get; set; } 
        public JobPriority Priority { get; set; }
        public JobStatus Status { get; set; }
        public CallBackStatus? CallBackStatus { get; set; }
        public DateTime? RequestSignatureApiDate { get; set; }
        public DateTime? ResponseSignatureApiDate { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}