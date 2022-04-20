//Diego Hiriart Leon
namespace WebAPI_DiegoHiriart.Models
{
    public class UserDto
    {
        public UserDto() { }

        public UserDto(long UserID, string Email, string Username, string Password)
        {
            this.UserID = UserID;
            this.Email = Email;
            this.Username = Username;
            this.Password = Password;
        }

        public long UserID { set; get; }
        public string Email { set; get; }
        public string Username { set; get; }
        public string Password { set; get; }       
    }
}
