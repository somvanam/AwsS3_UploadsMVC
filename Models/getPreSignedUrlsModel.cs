namespace AwsS3_UploadsMVC.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Part
    {
        public string signedUrl { get; set; }
        public int PartNumber { get; set; }
    }

    public class getPreSignedUrlsModel
    {
        public List<Part> parts { get; set; }
    }

}
