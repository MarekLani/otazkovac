using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CQA.Filters;
using CQA.Models;
using CQA.Resources;
using WebMatrix.WebData;

namespace CQA.Controllers
{
    public class SavedImage
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string RealName { get; set; }

        public static string CreateUploadedImagesStringFromSession(List<SavedImage> list)
        {
            string s ="";
            foreach (var image in list)
                s += String.Format("<div class=\"name\" id=" +
                "\"x{0}\">{1} {2}<br><form method=\"post\" action=\"Files/DeleteImage\" class=\"deleteImage\">" +
                "<input type=\"hidden\" name=\"Name\" value=\"{0}\">" +
                "<input type=\"submit\" value=\"Vymazať\"></form></div>", image.RealName, image.Name, image.Uri);
            return s;
        }

    }

     [InitializeSimpleMembership]
    public class FilesController : Controller
    {
        //
        // GET: /Files/

        private CQADBContext db = new CQADBContext();

        public ActionResult Index()
        {
            return View();
        }

        // This action handles the form POST and the upload
        [HttpPost]
        public string UploadImage(IEnumerable<HttpPostedFileBase> files)
        {
            //Saved to se how you make a json
            //List<Object> result = new List<Object>();
            string errors = "";
            foreach (var file in files)
            {

                // Verify that the user selected a file
                if (file != null && file.ContentLength > 0)
                {
                    var list = (List<SavedImage>)Session["uploadedImages"];

                    //control if image with same name is not already uploaded
                    if (list != null && list.Where((i) => i.Name == file.FileName).Any())
                    {
                        errors += String.Format(ErrorStrings.SameNameOfImage, file.FileName);
                        continue;
                    }

                    string extension = Path.GetExtension(file.FileName);
                    extension = extension.ToLower();
                    if (!FileIsWebFriendlyImage(file))
                    {
                        errors += String.Format(ErrorStrings.WrongImageType, file.FileName);
                        continue;
                    }

                    var img = SaveImage(file, "Articles");
                    //Saving to session   
                    if (list == null)
                        list = new List<SavedImage>();
                    list.Add(img);
                    Session["uploadedImages"] = list;

                    //THis is how the json is made
                    //result.Add(new { Status = "Ok", Name = file.FileName, Uri = uri, RealName = renamedImage });
                }
                else
                {
                    errors += String.Format(ErrorStrings.ImageUknownError, file.FileName);
                }

            }
           
            var l = (List<SavedImage>)Session["uploadedImages"];
            if (errors != "")
                errors = "<div class=\"uploadErrors\">" + errors + "</div>";
            if (l != null && l.Any())
                return errors + SavedImage.CreateUploadedImagesStringFromSession(l);
            else
                return errors;
        }

        [HttpPost]
        public ActionResult UploadImageToGallery(IEnumerable<HttpPostedFileBase> files)
        {
            //Saved to se how you make a json
            List<Object> result = new List<Object>();
            foreach (var file in files)
            {

                // Verify that the user selected a file
                if (file != null && file.ContentLength > 0)
                {

                    string extension = Path.GetExtension(file.FileName);
                    extension = extension.ToLower();
                    if (!FileIsWebFriendlyImage(file))
                    {
                        result.Add(new { Status = "WrongFormat", Name = file.FileName });
                        continue;
                    }


                    var img = SaveImage(file, "Galleries",true);

                    UploadedImage ui = new UploadedImage();
                    ui.ImageUrl = img.Uri;
                    ui.UploadedImageId = Guid.NewGuid();
                    //TODO change to current user resp. trainer
                    int userId = WebSecurity.CurrentUserId;
                    ui.Position = db.UserProfiles.Find(userId).Images.Count();
                    if (db.UserProfiles.Find(userId).Images == null)
                        db.UserProfiles.Find(userId).Images = new List<UploadedImage>();
                    db.UserProfiles.Find(userId).Images.Add(ui);
                    db.SaveChanges();


                    //THis is how the json is made
                    result.Add(new { Status = "Ok", Name = file.FileName, ThumbUri = GetThumbUri(img.Uri), Uri = img.Uri, ImageId = ui.UploadedImageId });
                }
                else
                {
                    result.Add(new { Status = "Error", Name = file.FileName});
                }

            }

            return Json(result);
        }

        private SavedImage SaveImage(HttpPostedFileBase file, string destination, bool createThumbnail = false)
        {
            string renamedImage = null;
            string extension = Path.GetExtension(file.FileName);
            // do
            // {
            renamedImage = System.Guid.NewGuid().ToString("N");
            // } while( Directory.GetFiles("Images/Uploads/",renamedImage).Any());

            //Create Thumbnail
            if(createThumbnail)
                CreateThumbnail(Image.FromStream(file.InputStream), 150, 150, true, renamedImage, destination);

            renamedImage += extension;
            file.SaveAs(Server.MapPath("~/Images/"+destination+"/") + renamedImage);

            SavedImage img = new SavedImage();
            img.Name = file.FileName;
            string uri = Request.Url.GetLeftPart(UriPartial.Authority) + "/Images/" + destination + "/" + renamedImage;
            img.Uri = uri;
            img.RealName = renamedImage;

            return img;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveImage(Guid Id1, Guid Id2)
        {
            //TODO for curent user
            if (WebSecurity.CurrentUserId != db.UploadedImages.Find(Id1).UserId || WebSecurity.CurrentUserId != db.UploadedImages.Find(Id2).UserId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            int tmp = db.UploadedImages.Find(Id1).Position;
            db.UploadedImages.Find(Id1).Position = db.UploadedImages.Find(Id2).Position;
            db.UploadedImages.Find(Id2).Position = tmp;
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteImageFromGallery(Guid Id)
        {
            //TODO for curent user
            if (WebSecurity.CurrentUserId != db.UploadedImages.Find(Id).UserId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            else{
                var delImage = db.UploadedImages.Find(Id);
                foreach (var image in db.UploadedImages.Where(i => i.Position > delImage.Position && i.UserId == WebSecurity.CurrentUserId))
                    image.Position -= 1;
                
                //delete Image
                var uri = new Uri(delImage.ImageUrl);
                var fileDest = Server.MapPath("~/Images/Galleries/") + uri.Segments[uri.Segments.Length - 1];
                System.IO.File.Delete(fileDest);

                //delete Thumb
                uri = new Uri(GetThumbUri(delImage.ImageUrl));
                fileDest = Server.MapPath("~/Images/Galleries/") + uri.Segments[uri.Segments.Length - 1];
                System.IO.File.Delete(fileDest);

                db.UploadedImages.Remove(delImage);

            }
            db.SaveChanges();        
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public static string GetThumbUri(string uri)
        {
            var thumbUri = uri.Substring(uri.LastIndexOf("/") + 1);
            thumbUri = thumbUri.Substring(0, thumbUri.LastIndexOf("."));
            return uri.Substring(0, uri.LastIndexOf("/")) + "/thumb_" + thumbUri + ".png";
        }

        [HttpPost]
        public ActionResult DeleteImage(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                string path = Server.MapPath("~/Images/Articles/") + Name;
                System.IO.File.Delete(path);

                var list = (List<SavedImage>)Session["uploadedImages"];
                //control if image with same name is not already uploaded
                if (list != null && list.Where((i) => i.RealName == Name).Any())
                {
                    var image = list.Where((i) => i.RealName == Name).First();
                    list.Remove(image);
                    Session["uploadedImages"] = list;
                }
            }
            return Json(new { Name = Name });
        }

        public static bool FileIsWebFriendlyImage(HttpPostedFileBase file)
        {
            try
            {
                //Read an image from the stream...
                var i = Image.FromStream(file.InputStream);
                file.InputStream.Seek(0, SeekOrigin.Begin);
                return ImageFormat.Jpeg.Equals(i.RawFormat) || ImageFormat.Png.Equals(i.RawFormat) || ImageFormat.Gif.Equals(i.RawFormat);
            }
            catch
            {
                return false;
            }
        }

        public void CreateThumbnail(Image imgPhoto, int Width, int Height, bool needToFill, string name, string dest)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (!needToFill)
            {
                if (nPercentH < nPercentW)
                {
                    nPercent = nPercentH;
                }
                else
                {
                    nPercent = nPercentW;
                }
            }
            else
            {
                if (nPercentH > nPercentW)
                {
                    nPercent = nPercentH;
                    destX = (int)Math.Round((Width -
                        (sourceWidth * nPercent)) / 2);
                }
                else
                {
                    nPercent = nPercentW;
                    destY = (int)Math.Round((Height -
                        (sourceHeight * nPercent)) / 2);
                }
            }

            if (nPercent > 1)
                nPercent = 1;

            int destWidth = (int)Math.Round(sourceWidth * nPercent);
            int destHeight = (int)Math.Round(sourceHeight * nPercent);

            System.Drawing.Bitmap bmPhoto = new System.Drawing.Bitmap(
                destWidth <= Width ? destWidth : Width,
                destHeight < Height ? destHeight : Height,
                              PixelFormat.Format32bppRgb);
            //bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
            //                 imgPhoto.VerticalResolution);

            System.Drawing.Graphics grPhoto = System.Drawing.Graphics.FromImage(bmPhoto);
            grPhoto.Clear(System.Drawing.Color.White);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;


            grPhoto.DrawImage(imgPhoto,
                new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
                new System.Drawing.Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                System.Drawing.GraphicsUnit.Pixel);

            grPhoto.Dispose();
            bmPhoto.Save(Server.MapPath("~/Images/"+dest+"/thumb_") + name + ".png", ImageFormat.Png); 
        }
    }
}
