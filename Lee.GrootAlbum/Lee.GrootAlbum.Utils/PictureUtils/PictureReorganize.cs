using Azylee.Core.DataUtils.GuidUtils;
using Azylee.Core.DataUtils.StringUtils;
using Azylee.Core.IOUtils.DirUtils;
using Azylee.Core.IOUtils.FileUtils;
using Azylee.Core.IOUtils.ImageUtils;
using Lee.GrootAlbum.Models.PictureModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static PictureModel CreateModel(string file)
        {
            try
            {
                DateTime createTime = File.GetCreationTime(file);
                DateTime writeTime = File.GetLastWriteTime(file);

                FileCodeTool codetool = new FileCodeTool();
                string md5 = codetool.GetMD5(file);
                string sha1 = codetool.GetSHA1(file);

                PictureModel picture = new PictureModel()
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

                    picture.GpsLongitudeRef = ex.GetPropertyChar((int)ExifTagNames.GpsLongitudeRef);
                    picture.GpsLatitudeRef = ex.GetPropertyChar((int)ExifTagNames.GpsLatitudeRef);
                    picture.GpsLongitude = ex.GetPropertyDouble((int)ExifTagNames.GpsLongitude) * (picture.GpsLongitudeRef.Equals('E') ? 1 : -1);
                    picture.GpsLatitude = ex.GetPropertyDouble((int)ExifTagNames.GpsLatitude) * (picture.GpsLatitudeRef.Equals('N') ? 1 : -1);

                    string[] exifDTOrig = ex.GetPropertyString((int)ExifTagNames.ExifDTOrig).Trim().Split(' ');
                    if (exifDTOrig != null && exifDTOrig.Count() == 2)
                    {
                        if (DateTime.TryParse(string.Format("{0} {1}", exifDTOrig[0].Replace(':', '-'), exifDTOrig[1]), out DateTime dt)) picture.OrigTime = dt;
                    }
                    picture.Name = string.Format("{0}-{1}.{2}", picture.OrigTime.ToString("yyyyMMddhhmmss"), picture.MD5, picture.ExtName);
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
        public static PictureModel AddLocationInfo(string file, PictureModel picture)
        {
            return picture;
        }
        /// <summary>
        /// 添加照片内容识别信息
        /// </summary>
        /// <param name="picture"></param>
        /// <returns></returns>
        public static PictureModel AddContentInfo(string file, PictureModel picture)
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
        public static bool ReorganizePicture(string file, string path, PictureModel picture)
        {
            try
            {
                if (File.Exists(file))
                {
                    //创建缩略图
                    string thumb = DirTool.Combine(path, "thumb", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}", $"{picture.Model}",picture.Name);
                    //创建压缩图
                    string normal = DirTool.Combine(path, "normal", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}", $"{picture.Model}", picture.Name);
                    //整理原始照片
                    string high = DirTool.Combine(path, "high", $"{picture.OrigTime.Year}-{picture.OrigTime.Month}", $"{picture.Model}", picture.Name);
                    return true;
                }
            }
            catch { return false; }
            return false;
        }
    }
}
