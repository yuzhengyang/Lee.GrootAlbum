using Lee.GrootAlbum.Models.DBModels;
using Lee.GrootAlbum.Models.PictureModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace Azylee.DB.SQLite.Mappings
{
    public class PictureModelMap : EntityTypeConfiguration<Pictures>
    {
        public PictureModelMap()
        {

        }
    }
}
