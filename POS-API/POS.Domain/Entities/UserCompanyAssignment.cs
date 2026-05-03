using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class UserCompanyAssignment : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public int RoleId { get; set; }
        public Guid? LocationId { get; set; }

        public User? User { get; set; }
        public Company? Company { get; set; }
        public Role? Role { get; set; }
        public Location? Location { get; set; }
    }
}
