using AwsS3_UploadsMVC.Models;

namespace AwsS3_UploadsMVC.Interfaces
{
    public interface IAwsS3Upload
    {
       // public Task<JWToken> GetWToken();
        public Task<string> AwsS3Upload(IFormFile file);
    }
}
