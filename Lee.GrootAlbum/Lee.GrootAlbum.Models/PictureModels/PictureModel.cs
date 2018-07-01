using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lee.GrootAlbum.Models.PictureModels
{
    /// <summary>
    /// 图片信息模型
    /// </summary>
    public class PictureModel
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// 图片名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 图片扩展名
        /// </summary>
        public string ExtName { get; set; }
        /// <summary>
        /// 位置信息
        /// </summary>
        public string Place { get; set; }
        /// <summary>
        /// MD5码信息
        /// </summary>
        public string MD5 { get; set; }
        /// <summary>
        /// SHA1码信息
        /// </summary>
        public string SHA1 { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime OrigTime { get; set; }
        /// <summary>
        /// 设备型号信息
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 经度 方向
        /// </summary>
        public char GpsLongitudeRef { get; set; }
        /// <summary>
        /// 纬度 方向
        /// </summary>
        public char GpsLatitudeRef { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public double GpsLongitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double GpsLatitude { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 收藏
        /// </summary>
        public int Star { get; set; }
    }
}
