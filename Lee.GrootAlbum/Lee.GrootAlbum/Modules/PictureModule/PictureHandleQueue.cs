using Azylee.Core.IOUtils.FileUtils;
using Azylee.Core.ThreadUtils.SleepUtils;
using Azylee.DB.SQLite.Engine;
using Lee.GrootAlbum.Commons;
using Lee.GrootAlbum.Models.DBModels;
using Lee.GrootAlbum.Models.PictureModels;
using Lee.GrootAlbum.Utils.PictureUtils;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lee.GrootAlbum.Modules.PictureModule
{
    public static class PictureHandleQueue
    {
        static bool IsStart = false;
        static short Interval = 5;
        public static CancellationTokenSource Token = new CancellationTokenSource();
        private static ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();
        public static void Add(string file)
        {
            try
            {
                if (Queue.Any(x => x == file))
                {
                    //重复图片
                }
                else
                {
                    Queue.Enqueue(file);
                }
            }
            catch { }
        }
        public static void Start()
        {
            if (IsStart) return;
            IsStart = true;

            Task.Factory.StartNew(() =>
            {
                //设置退出条件
                while (!Token.IsCancellationRequested)
                {
                    //队列中存在元素
                    if (Queue.Any())
                    {
                        //循环进行操作
                        for (int i = 0; i < Queue.Count; i++)
                        {
                            try
                            {
                                if (Queue.TryDequeue(out string file))
                                {
                                    R.Log.v("💗💗💗 准备处理文件：" + file);
                                    var pic = PictureReorganize.CreateModel(file);
                                    if (pic != null)
                                    {
                                        R.Log.v("图片信息读取成功");

                                        if (PictureReorganize.Exist(file, R.Paths.Pictures, pic))
                                        {
                                            R.Log.v("图片已入库，不需要在重复保存了，即将删除");
                                            FileTool.Delete(file);
                                        }
                                        else
                                        {
                                            using (Muse db = new Muse("pictures"))
                                            {
                                                if (db.Any<Pictures>(x => x.MD5 == pic.MD5 && x.SHA1 == pic.SHA1, null))
                                                {
                                                    R.Log.v("图片已入库，不需要在重复保存了，即将删除");
                                                    FileTool.Delete(file);
                                                }
                                                else
                                                {
                                                    R.Log.v("图片未入库，准备入库并分类保存");
                                                    if (db.Add(pic) > 0)
                                                    {
                                                        //pic = PictureReorganize.AddLocationInfo(pic);
                                                        //pic = PictureReorganize.AddContentInfo(pic);
                                                        PictureReorganize.ReorganizePicture(file, R.Paths.Pictures, pic);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    Sleep.S(Interval);
                }
            });
        }
    }
}
