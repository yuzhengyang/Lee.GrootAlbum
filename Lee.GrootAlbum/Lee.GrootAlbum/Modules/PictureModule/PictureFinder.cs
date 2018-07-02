using Azylee.Core.DataUtils.CollectionUtils;
using Azylee.Core.IOUtils.FileUtils;
using Azylee.Core.ThreadUtils.SleepUtils;
using Lee.GrootAlbum.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lee.GrootAlbum.Modules.PictureModule
{
    public class PictureFinder
    {
        public static void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    List<string> files = FileTool.GetAllFile(R.Paths.Unsorted, new string[] { "*.jpg", "*.jpeg" });
                    if (ListTool.HasElements(files))
                    {
                        foreach (var file in files)
                        {
                            string ext = Path.GetExtension(file).ToLower();
                            if (ext.Contains("jpg") || ext.Contains("jpeg"))
                                PictureHandleQueue.Add(file);
                        }
                    }
                    Sleep.S(10);
                }
            });
        }
    }
}
