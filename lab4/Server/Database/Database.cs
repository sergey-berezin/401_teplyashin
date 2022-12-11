using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using ClassLibrary2;

namespace Server.Database
{
    public class LibraryContext : DbContext
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<Hash> Hashes { get; set; }
        public DbSet<ByteImage> ByteImages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite("Data Source=images.db");
    }

    public interface IImageDb
    {
        Task<int> PostImage(byte[] img_, string path, CancellationToken token);
        Task<List<int>> GetAllImages();
        Task<Image?> GetImageById(int id);
        Task<int> DeleteAllImages();
    }

    public class Methods : IImageDb
    {
        Class1 cls = new Class1();
        public async Task<int> PostImage(byte[] img_, string path, CancellationToken token)
        {
            try
            {
                using (var db = new LibraryContext())
                {
                    var hash_ = img_.GetHashCode();
                    if (!db.Hashes.Any(x => x.hash == hash_))
                    {
                        if (!db.ByteImages.Any(x => x.byteImage.Equals(img_)))
                        {
                            var task_ = cls.process(path, token);
                            await task_;
                            (string, float)[] res = task_.Result;
                            var res_list = new List<(float, string)>();
                            for (int j = 0; j < res.Length; j++)
                                res_list.Add((res[j].Item2, res[j].Item1));
                            res_list.Sort();
                            res_list.Reverse();
                            if (res_list[0].Item1 == 0)
                                return -1;
                            Image img = new Image(path, res_list);
                            db.Images.Add(img);
                            db.SaveChanges();
                            int id = db.Images.OrderByDescending(x => x.imageID).First().imageID;
                            db.Hashes.Add(new Hash { imageID = id, hash = hash_ });
                            db.ByteImages.Add(new ByteImage { imageID = id, byteImage = img_ });
                            db.SaveChanges();
                            return id;
                        }
                    }
                    return db.Hashes.Where(x => x.hash == hash_).First().imageID;
                }
            }
            catch (OperationCanceledException e)
            {
                return -1;
            }
        }
        public async Task<List<int>> GetAllImages()
        {
            var res = new List<int>();
            using (var db = new LibraryContext())
            {
                foreach (var img in db.Images)
                {
                    res.Add(img.imageID);
                }
            }
            return res;
        }
        public async Task<Image?> GetImageById(int id)
        {
            Image? img = null;
            using (var db = new LibraryContext())
            {
                var query = db.Images.Where(x => x.imageID == id);
                if (query.Any())
                    img = query.First();
            }
            return img;
        }
        public async Task<int> DeleteAllImages()
        {
            using (var db = new LibraryContext())
            {
                foreach (var img in db.Images)
                {
                    int id = img.imageID;
                    db.Images.Remove(img);
                    var query_1 = db.Hashes.Where(x => x.imageID == id);
                    db.Hashes.Remove(query_1.First());
                    var query_2 = db.ByteImages.Where(x => x.imageID == id);
                    db.ByteImages.Remove(query_2.First());
                    db.SaveChanges();
                }
            }
            return 0;
        }
    }
}