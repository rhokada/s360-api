using System;

namespace WebApi.Models
{
    public class AdmRoleUserModel
    {
        public int      AdmRoleUserId { get; set; }
        public int      AdmRoleId     { get; set; }
        public string   AdmRoleName   { get; set; }
        public string   AdmRoleCd     { get; set; }
        public int      UserId        { get; set; }
        public string   UserName      { get; set; }
        public string   UserEmail     { get; set; }
        public DateTime? DhCreate     { get; set; }
    }

    public class AdmRoleUserFilterModel
    {
        public int? AdmRoleId { get; set; }
        public int? UserId    { get; set; }
    }

    public class AdmRoleUserCreateModel
    {
        public int AdmRoleId { get; set; }
        public int UserId    { get; set; }
    }
}
