using _16Nov_task.Models;

namespace _16Nov_task.Utilities.Extensions
{
    public static class FileValidator
    {
        public static bool ValidateType(this IFormFile file,string type="image/")
        {
            if (file.ContentType.Contains(type))
            {
                return true;
            }
            return false;
        }

        public static bool ValidateSize(this IFormFile file, int LimitKb)
        {
            if(file.Length <= LimitKb * 1024)
            {
                return true;
            }
            return false;
        }

        public static async Task<string> CreateFileAsync (this IFormFile file,string root,params string[] folders) 
        {
            string fileName = Guid.NewGuid().ToString() + file.FileName;
            string path = root;
            for (int i = 0; i < folders.Length; i++)
            {
                path = Path.Combine(path, folders[i]);
            }

            path=Path.Combine(path, fileName);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
           
        }

        public static async void DeleteFile(this string fileName, string root, params string[] folders)
        {
            string path = root;

            for(int i = 0;i < folders.Length;i++)
            {
                path=Path.Combine(path, folders[i]);
            }
            path=Path.Combine(path,fileName);
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
