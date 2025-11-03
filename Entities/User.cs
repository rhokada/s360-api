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
}