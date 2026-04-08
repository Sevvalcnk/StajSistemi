using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StajSistemi.Models; // Namespace'in doğru olduğundan emin ol kardaşım

namespace StajSistemi.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        // ✅ DÜZELTME: string yerine int yapıyoruz (Senin AppUser yapına uygun)
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime SentDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public int? SuggestedInternshipId { get; set; }
        public virtual Internship SuggestedInternship { get; set; }

        // 🔥 KRİTİK MÜHÜR: DbContext'in hata vermesini engelleyen kısımlar:
        [ForeignKey("SenderId")]
        public virtual AppUser Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual AppUser Receiver { get; set; }
    }
}