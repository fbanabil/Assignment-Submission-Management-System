using AssignmentSystem.Api.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsUsed { get; set; }

        public Guid UserId { get; set; }

        // Navigation property to the User entity
        public virtual User? User { get; set; } = null!;
    }
}
