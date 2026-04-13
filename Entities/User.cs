namespace WebApi.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int? RoleId { get; set; }
        public int? CompanyId { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Msg { get; set; }
    }

    public class UserAdm : User
    {
        public List<UserRole> Roles { get; set; }
    }

    public class UserRole
    {
        public int AdmRoleId { get; set; }
        public string AdmRoleCd { get; set; }
        public string AdmRoleName { get; set; }
        public List<RolePermissions> RolePermissions { get; set; } = null;
    }

    public class RolePermissions 
    {
        public string Slug { get; set; }
        public string Menu { get; set; }
        public string Icon { get; set; }
        public bool Read { get; set; }
        public bool Create { get; set; }
        public bool Delete { get; set; }
        public bool Alter { get; set; }
    }
}