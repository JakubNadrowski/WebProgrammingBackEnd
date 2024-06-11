namespace CampingProject.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int spotId { get; set; }

        public int rating {  get; set; }

        public string comment { get; set; }
    }
}
