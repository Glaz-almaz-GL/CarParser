using HaynesProParser.DataBase.Items;
using Microsoft.EntityFrameworkCore;

namespace HaynesProParser.DataBase
{
    public class AppDbContext : DbContext
    {
        public DbSet<dbTransportTypeData> TransportTypes { get; set; }
        public DbSet<dbMakerData> Makers { get; set; }
        public DbSet<dbModelData> Models { get; set; }
        public DbSet<dbModificationData> Modifications { get; set; }
        public DbSet<dbGuideTypeData> GuideTypes { get; set; }
        public DbSet<dbMainGuideData> MainGuides { get; set; }
        public DbSet<dbGuideData> Guides { get; set; }
        public DbSet<dbJobData> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<dbTransportTypeData>()
                .HasMany(t => t.Makers)
                .WithOne(t => t.TransportType)
                .HasForeignKey(t => t.TransportTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<dbMakerData>()
                .HasMany(m => m.Models)                 // У макри может быть много моделей
                .WithOne(m => m.Maker)                  // У модели может быть одна марка
                .HasForeignKey(m => m.MakerId)               // Внешний ключ марки
                .OnDelete(DeleteBehavior.Restrict);     // При удалении удаляются все модификации

            modelBuilder.Entity<dbModelData>()
                .HasMany(m => m.Modifications)          // У модели может быть много модификаций
                .WithOne(m => m.Model)                  // У модификации может быть одна модель
                .HasForeignKey(m => m.ModelId)               // Внешний ключ модели
                .OnDelete(DeleteBehavior.Cascade);      // При удалении удаляются все модификации

            modelBuilder.Entity<dbModificationData>()
                .HasMany(m => m.GuideTypes)             // У модфификации может быть много гайдов
                .WithOne(m => m.Modification)           // У гайда может быть одна модификация
                .HasForeignKey(m => m.ModificationId)               // Внешний ключ модификации
                .OnDelete(DeleteBehavior.Cascade);      // При удалении удаляются все гайды

            modelBuilder.Entity<dbGuideTypeData>()
                .HasMany(g => g.MainGuides)             // У типа гайда может быть много главных гайдов
                .WithOne(g => g.GuideType)              // У типа главного гайда может быть один тип гайда
                .HasForeignKey(g => g.GuideTypeId)               // Внешний ключ типа гайда
                .OnDelete(DeleteBehavior.Cascade);      // При удалении удаляются все главные гайды

            modelBuilder.Entity<dbMainGuideData>()
                .HasMany(mg => mg.Guides)               // У главного гайда может быть много гайдов
                .WithOne(mg => mg.MainGuide)            // У гайда может быть один главный гайд
                .HasForeignKey(mg => mg.MainGuideId)             // Внешний ключ главного гайда
                .OnDelete(DeleteBehavior.Cascade);      // При удалении удаляются все гайды

            modelBuilder.Entity<dbGuideData>()
                .HasMany(g => g.Jobs)                   // У гайда может быть много работ
                .WithOne(g => g.Guide)                  // У работы может быть один гайд
                .HasForeignKey(g => g.GuideId)               // Внешний ключ гайда
                .OnDelete(DeleteBehavior.Cascade);      // При удалении удаляются все работы
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=haynesPro.db"); // Сохранение бд в файл
        }
    }
}
