namespace CampingProject.Models
{
    public class CampingSpot
    {
        public int id { get; set; }

        public int owner { get; set; }

        public string name { get; set; }
        public string location { get; set; }

        public string description { get; set; }

        public int capacity { get; set; }

        public int price { get; set; }

        public List<IFormFile>? Images { get; set; }

        public List <string>? imagePaths { get; set; }
    }
}
