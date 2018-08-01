using Azylee.Core.DataUtils.GuidUtils;
using Azylee.Core.DataUtils.StringUtils;
using Azylee.Core.IOUtils.DirUtils;
using Azylee.Core.IOUtils.ExifUtils;
using Azylee.Core.IOUtils.FileUtils;
using Azylee.Core.IOUtils.ImageUtils;
using Azylee.Core.IOUtils.TxtUtils;
using Azylee.Core.Plus.DataUtils.JsonUtils;
using Azylee.YeahWeb.BaiDuWebAPI.GPSAPI;
using Lee.GrootAlbum.Models.DBModels;
using Lee.GrootAlbum.Models.PictureModels;
using System;
using System.IO;
using System.Linq;

namespace Lee.GrootAlbum.Utils.PictureUtils
{
    public static class PictureReorganize
    {

        const string Unknown = "Un";//未知型号相机

        /// <summary>
        /// 创建照片信息模型
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Pictures CreateModel(string file)
        {
            try
            {
                DateTime createTime = File.GetCreationTime(file);
                DateTime writeTime = File.GetLastWriteTime(file);

                FileCodeTool codetool = new FileCodeTool();
                string md5 = codetool.GetMD5(file);
                string sha1 = codetool.GetSHA1(file);

                Pictures picture = new Pictures()
                {
                    Id = GuidTool.Short(),
                    ExtName = Path.GetExtension(file).ToUpper(),
                    MD5 = md5,
                    SHA1 = sha1,
                    OrigTime = createTime < writeTime ? createTime : writeTime,
                };

                using (ExifHelper ex = new ExifHelper(file))
                {
                    string maker = ex.GetPropertyString((int)ExifTagNames.EquipMake).Trim().Replace(" ", "_");
                    string model = ex.GetPropertyString((int)ExifTagNames.EquipModel).Trim().Replace(" ", "_");
                    picture.Model = (Str.Ok(maker) ? maker : Unknown) + "@" + (Str.Ok(model) ? model : Unknown);

                    char GpsLongitudeRef = ex.GetPropertyChar((int)ExifTagNames.GpsLongitudeRef);
                    char GpsLatitudeRef = ex.GetPropertyChar((int)ExifTagNames.GpsLatitudeRef);
                    picture.GpsLongitude = ex.GetPropertyDouble((int)ExifTagNames.GpsLongitude) * (GpsLongitudeRef.Equals('E') ? 1 : -1);
                    picture.GpsLatitude = ex.GetPropertyDouble((int)ExifTagNames.GpsLatitude) * (GpsLatitudeRef.Equals('N') ? 1 : -1);

                    string[] exifDTOrig = ex.GetPropertyString((int)ExifTagNames.ExifDTOrig).Trim().Split(' ');
                    if (exifDTOrig != null && exifDTOrig.Count() == 2)
                    {
                        if (DateTime.TryParse(string.Format("{0} {1}", exifDTOrig[0].Replace(':', '-'), exifDTOrig[1]), out DateTime dt)) picture.OrigTime = dt;
                    }
                    picture.Name = string.Format("{0}-{1}{2}", picture.OrigTime.ToString("yyyyMMddhhmmss"), picture.MD5, picture.ExtName);
                }
                return picture;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 添加照片位置信息
        /// </summary>
        /// <param name="picture"></param>
        /// <returns></returns>
        public static Pictures AddLocationInfo(Pictures picture)
        {
            try
            {
                GPSInfoWebModel model = GPSInfoTool.GetInfo("", picture.GpsLongitude, picture.GpsLatitude);
                string loc = model.ToGPSInfoModel().ToString();
                picture.Location = loc;
            }
            catch { }
            return picture;
        }
        /// <summary>
        /// 添加照片内容识别信息
        /// </summary>
        /// <param name="picture"></param>
        /// <returns></returns>
        public static Pictures AddContentInfo(Pictures picture)
        {
            return picture;
        }
        /// <summary>
        /// 整理图片到指定位置
        /// </summary>
        /// <param name="file">图片路径</param>
        /// <param name="path">整理目标路径</param>
        /// <param name="picture">图片信息模型</param>
        /// <returns></returns>
        public static bool ReorganizePicture(string file, string path, Pictures picture)
        {
            try
            {
                if (File.Exists(file))
                {
                    //根据照片信息旋转，生成临时文件
                    string temp = DirTool.Combine(path, "_data", "temp");
                    DirTool.Create(temp);
                    string tempfile = DirTool.Combine(temp, picture.Name);
                    if (File.Exists(tempfile)) FileTool.Delete(tempfile);
                    RotateImageTool.Rotate(file, tempfile);

                    //创建压缩图
                    string normal = DirTool.Combine(path, "_data", "normal", $"{picture.Model}", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}");
                    DirTool.Create(normal);
                    if (File.Exists(DirTool.Combine(normal, picture.Name))) FileTool.Delete(DirTool.Combine(normal, picture.Name));
                    ImageHelper.MakeThumbnail(tempfile, DirTool.Combine(normal, picture.Name), 1000, 1000, "H");

                    //创建缩略图
                    string thumb = DirTool.Combine(path, "_data", "thumb", $"{picture.Model}", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}");
                    DirTool.Create(thumb);
                    if (File.Exists(DirTool.Combine(thumb, picture.Name))) FileTool.Delete(DirTool.Combine(thumb, picture.Name));
                    ImageHelper.MakeThumbnail(tempfile, DirTool.Combine(thumb, picture.Name), 500, 500, "Cut");

                    //整理原始照片
                    string original = DirTool.Combine(path, $"{picture.Model}", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}");
                    DirTool.Create(original);
                    if (File.Exists(DirTool.Combine(original, picture.Name))) FileTool.Delete(DirTool.Combine(original, picture.Name));
                    File.Move(file, DirTool.Combine(original, picture.Name));

                    //整理照片基础信息
                    string info = DirTool.Combine(path, "_data", "info", $"{picture.Model}", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}");
                    DirTool.Create(info);
                    TxtTool.Create(DirTool.Combine(info, picture.Name + ".info"), JsonTool.ToStr(picture));

                    FileTool.Delete(tempfile);
                    return true;
                }
            }
            catch { return false; }
            return false;
        }
        public static bool Exist(string file, string path, Pictures picture)
        {
            string normal = DirTool.Combine(DirTool.Combine(path, "_data", "normal", $"{picture.Model}", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}"), picture.Name);
            string thumb = DirTool.Combine(DirTool.Combine(path, "_data", "thumb", $"{picture.Model}", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}"), picture.Name);
            string original = DirTool.Combine(DirTool.Combine(path, $"{picture.Model}", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}"), picture.Name);
            string info = DirTool.Combine(DirTool.Combine(path, "_data", "info", $"{picture.Model}", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}"), picture.Name + ".info");
            if (File.Exists(normal) && File.Exists(thumb) && File.Exists(original) && File.Exists(info))
            {
                return true;
            }
            return false;
        }
    }
}
