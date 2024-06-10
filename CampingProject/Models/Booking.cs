namespace CampingProject.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int userID { get; set; }

        public int spotID { get; set; }

        public DateOnly startDate { get; set; }

        public DateOnly endDate { get; set; }
    }
}
