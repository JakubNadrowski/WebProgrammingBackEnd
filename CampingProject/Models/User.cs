using System;

namespace CampingProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string fName { get; set; }
        public string lName { get; set; }

        public int isOwner { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }

    public class UserLoginRequest
    {
        public string email { set; get; }
        public string password { set; get; }
    }
}
