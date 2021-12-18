using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KumportAPI.Post
{
    [Table("Posts")]
    public class PostModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }
        [MaxLength(100)]
        public string PostTitle { get; set; }
        [MaxLength(100)]
        public string FileType { get; set; }
        public string PostOwner { get; set; }
        [MaxLength]
        public byte[] Image { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
