using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESignature.DAL.Models
{
    [Table("ES_Medias")]
    public class Media : BaseEntity
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public JobFileType JobFileType { get; set; }
        public string Path { get; set; }
        public Guid? JobId { get; set; }
        public Guid? JobHistoryId { get; set; }

        [ForeignKey("JobHistoryId")]
        public JobHistory JobHistory { get; set; }

        [ForeignKey("JobId")]
        public Job Job { get; set; }
    }
}