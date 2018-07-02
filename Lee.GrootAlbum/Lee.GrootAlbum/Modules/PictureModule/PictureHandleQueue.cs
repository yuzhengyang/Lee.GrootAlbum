using Azylee.Core.ThreadUtils.SleepUtils;
using Azylee.DB.SQLite.Engine;
using Lee.GrootAlbum.Commons;
using Lee.GrootAlbum.Models.PictureModels;
using Lee.GrootAlbum.Utils.PictureUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lee.GrootAlbum.Modules.PictureModule
{
    public static class PictureHandleQueue
    {
        static bool IsStart = false;
        static short Interval = 5;
        public static CancellationToken CancelToken;
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
                while (!CancelToken.IsCancellationRequested)
                {
                    //如果通信正常，并且队列中存在元素
                    if (Queue.Any())
                    {
                        //循环进行操作
                        for (int i = 0; i < Queue.Count; i++)
                        {
                            try
                            {
                                if (Queue.TryDequeue(out string file))
                                {
                                    var pic = PictureReorganize.CreateModel(file);
                                    using (Muse db = new Muse("pictures"))
                                    {
                                        var rec = db.Get<PictureModel>(x => x.MD5 == pic.MD5 && x.SHA1 == pic.SHA1, null);
                                        if (rec == null)
                                        {
                                            pic = PictureReorganize.AddLocationInfo(file, pic);
                                            pic = PictureReorganize.AddContentInfo(file, pic);
                                            PictureReorganize.ReorganizePicture(file, R.Paths.Pictures, pic);
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
