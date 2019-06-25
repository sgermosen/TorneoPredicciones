using System.IO;
using System.Web;

namespace API.Classes
{
    public class FilesHelper
    {
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }


        public static string UploadPhoto(HttpPostedFileBase file, string folder)
        {
            var pic = string.Empty;

            if (file == null) return pic;
            pic = Path.GetFileName(file.FileName);
            var path = Path.Combine(HttpContext.Current.Server.MapPath(folder), pic);
            // path = Path.Combine(HttpContent.Current.Server.MapPath(folder), pic);                
            file.SaveAs(path);
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    file.InputStream.CopyTo(ms);
            //    byte[] array = ms.GetBuffer();
            //}

            return pic;
        }

        public static bool UploadPhoto(MemoryStream stream, string folder, string name)
        {

            try
            {

                stream.Position = 0;
                var path = Path.Combine(HttpContext.Current.Server.MapPath(folder), name);
                File.WriteAllBytes(path, stream.ToArray());

            }
            catch (System.Exception)
            {

                return false;

            }

            return true;

        }
    }

}