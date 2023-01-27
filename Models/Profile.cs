namespace WebAPI_DiegoHiriart.Models
{
    public class Profile
    {
        public Profile() { }

        public Profile(Int64 userid, string firstname, string lastname, string bio, bool isadmin, bool localAccount)
        {
            this.UserId = userid;
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.Bio = bio;
            this.IsAdmin = isadmin;
            this.localAccount = localAccount;
        }

        public Int64 UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Bio { get; set; }
        public bool IsAdmin { get; set; }
        public bool localAccount { get; set; }
    }
}
