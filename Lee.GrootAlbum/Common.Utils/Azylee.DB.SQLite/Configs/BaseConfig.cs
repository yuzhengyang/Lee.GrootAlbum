using Lee.GrootAlbum.Models.DBModels;
using Lee.GrootAlbum.Models.PictureModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azylee.DB.SQLite.Configs
{
    public class BaseConfig : DbConfiguration
    {
        public static void Configuer(DbModelBuilder modelBuilder)
        {
            ConfiguerUserEntity(modelBuilder);
        }
        private static void ConfiguerUserEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pictures>().HasKey(x => x.Id);
        }
    }
}
