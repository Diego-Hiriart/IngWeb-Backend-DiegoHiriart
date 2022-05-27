namespace WebAPI_DiegoHiriart.Models
{
    public class Profile
    {
        public Profile() { }

        public Profile(UInt64 userid, string firstname, string lastname, string bio, bool isadmin)
        {
            this.UserId = userid;
            this.Firstname = firstname; 
            this.Lastname = lastname;
            this.Bio = bio;
            this.IsAdmin = isadmin;
        }

        public UInt64 UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Bio { get; set; }
        public bool IsAdmin { get; set; }
    }
}
