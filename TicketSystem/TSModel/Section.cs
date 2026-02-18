using System.ComponentModel.DataAnnotations;
using TicketSystem.Data;

namespace TicketSystem.TSModel
{
    public class Section
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم القسم مطلوب.")]
        public string Name { get; set; }


        public ICollection<UserSection> UserSections { get; set; } = new List<UserSection>();


    }
}
