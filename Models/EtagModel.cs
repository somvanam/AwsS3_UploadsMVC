namespace AwsS3_UploadsMVC.Models
{
    public class Part1
    {
        public int PartNumber { get; set; }
        public string ETag { get; set; }
    }

  
    public class EtagModel
    {
        public List<Part1> parts { get; set; }
    }
}
