namespace WebAPI_DiegoHiriart.Models
{
    public class User
    {
        public User() { }

        public User(long UserID, string Email, string Username, byte[] PasswordHash, byte[] PasswordSalt)
        {
            this.UserID = UserID;
            this.Email = Email;
            this.Username = Username;
            this.PasswordHash = PasswordHash;
            this.PasswordSalt = PasswordSalt;         
        }
        
        public long UserID { set; get; }
        public string Email { set; get; } = string.Empty;
        public byte[] PasswordHash { set; get; }
        public byte[] PasswordSalt { set; get; }
        public string Username { set; get; } = string.Empty;
    }
}
