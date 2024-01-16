namespace TPPizza.WEB.Utils
{
    public static class FormFileExt
    {
        public static async Task<byte[]> GetAllBytesAsync(this IFormFile file)
        {
            using MemoryStream ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
