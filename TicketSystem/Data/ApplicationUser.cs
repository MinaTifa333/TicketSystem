using Microsoft.AspNetCore.Identity;
using TicketSystem.TSModel;

namespace TicketSystem.Data
{
    public class ApplicationUser : IdentityUser
    {
        public UserProfile? Profile { get; set; }
        public ICollection<Section> UserOutlets { get; set; } = new List<Section>();
        public ICollection<UserSection> UserSections { get; set; } = new List<UserSection>();
        public bool CanCloseOrder { get; set; } = false;
    }
}
