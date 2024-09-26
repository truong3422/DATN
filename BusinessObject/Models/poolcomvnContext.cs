using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace BusinessObject.Models
{
    public partial class poolcomvnContext : DbContext
    {
        public poolcomvnContext()
        {
        }

        public poolcomvnContext(DbContextOptions<poolcomvnContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<AdministrativeRegion> AdministrativeRegions { get; set; } = null!;
        public virtual DbSet<AdministrativeUnit> AdministrativeUnits { get; set; } = null!;
        public virtual DbSet<Club> Clubs { get; set; } = null!;
        public virtual DbSet<ClubPost> ClubPosts { get; set; } = null!;
        public virtual DbSet<Contact> Contacts { get; set; } = null!;
        public virtual DbSet<Country> Countries { get; set; } = null!;
        public virtual DbSet<District> Districts { get; set; } = null!;
        public virtual DbSet<GameType> GameTypes { get; set; } = null!;
        public virtual DbSet<MatchOfTournament> MatchOfTournaments { get; set; } = null!;
        public virtual DbSet<News> News { get; set; } = null!;
        public virtual DbSet<Player> Players { get; set; } = null!;
        public virtual DbSet<PlayerInMatch> PlayerInMatches { get; set; } = null!;
        public virtual DbSet<PlayerInSoloMatch> PlayerInSoloMatches { get; set; } = null!;
        public virtual DbSet<PlayerType> PlayerTypes { get; set; } = null!;
        public virtual DbSet<Province> Provinces { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<SoloMatch> SoloMatches { get; set; } = null!;
        public virtual DbSet<Table> Tables { get; set; } = null!;
        public virtual DbSet<Tournament> Tournaments { get; set; } = null!;
        public virtual DbSet<TournamentType> TournamentTypes { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Ward> Wards { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("PoolCom"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => e.RoleId, "IX_Accounts_RoleID");

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.VerifyCode).HasColumnName("verifyCode");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AdministrativeRegion>(entity =>
            {
                entity.ToTable("administrative_regions");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CodeName)
                    .HasMaxLength(255)
                    .HasColumnName("code_name");

                entity.Property(e => e.CodeNameEn)
                    .HasMaxLength(255)
                    .HasColumnName("code_name_en");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(255)
                    .HasColumnName("name_en");
            });

            modelBuilder.Entity<AdministrativeUnit>(entity =>
            {
                entity.ToTable("administrative_units");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CodeName)
                    .HasMaxLength(255)
                    .HasColumnName("code_name");

                entity.Property(e => e.CodeNameEn)
                    .HasMaxLength(255)
                    .HasColumnName("code_name_en");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.FullNameEn)
                    .HasMaxLength(255)
                    .HasColumnName("full_name_en");

                entity.Property(e => e.ShortName)
                    .HasMaxLength(255)
                    .HasColumnName("short_name");

                entity.Property(e => e.ShortNameEn)
                    .HasMaxLength(255)
                    .HasColumnName("short_name_en");
            });

            modelBuilder.Entity<Club>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "UQ_AccountID")
                    .IsUnique();

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.WardCode)
                    .HasMaxLength(20)
                    .HasColumnName("Ward_code");

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.Club)
                    .HasForeignKey<Club>(d => d.AccountId)
                    .HasConstraintName("FK_Clubs_Accounts");

                entity.HasOne(d => d.WardCodeNavigation)
                    .WithMany(p => p.Clubs)
                    .HasForeignKey(d => d.WardCode)
                    .HasConstraintName("FK_Clubs_Wards");
            });

            modelBuilder.Entity<ClubPost>(entity =>
            {
                entity.HasKey(e => e.PostId);

                entity.HasIndex(e => e.ClubId, "IX_ClubPosts_ClubID");

                entity.Property(e => e.PostId).HasColumnName("PostID");

                entity.Property(e => e.ClubId).HasColumnName("ClubID");

                entity.Property(e => e.Flyer).HasMaxLength(255);

                entity.Property(e => e.Link).HasMaxLength(255);

                entity.HasOne(d => d.Club)
                    .WithMany(p => p.ClubPosts)
                    .HasForeignKey(d => d.ClubId);
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.Property(e => e.ContactId).HasColumnName("Contact_Id");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.FullName).HasMaxLength(255);
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.CountryImage).HasMaxLength(255);

                entity.Property(e => e.CountryName).HasMaxLength(255);
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("districts_pkey");

                entity.ToTable("districts");

                entity.HasIndex(e => e.ProvinceCode, "idx_districts_province");

                entity.HasIndex(e => e.AdministrativeUnitId, "idx_districts_unit");

                entity.Property(e => e.Code)
                    .HasMaxLength(20)
                    .HasColumnName("code");

                entity.Property(e => e.AdministrativeUnitId).HasColumnName("administrative_unit_id");

                entity.Property(e => e.CodeName)
                    .HasMaxLength(255)
                    .HasColumnName("code_name");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.FullNameEn)
                    .HasMaxLength(255)
                    .HasColumnName("full_name_en");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(255)
                    .HasColumnName("name_en");

                entity.Property(e => e.ProvinceCode)
                    .HasMaxLength(20)
                    .HasColumnName("province_code");

                entity.HasOne(d => d.AdministrativeUnit)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(d => d.AdministrativeUnitId)
                    .HasConstraintName("districts_administrative_unit_id_fkey");

                entity.HasOne(d => d.ProvinceCodeNavigation)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(d => d.ProvinceCode)
                    .HasConstraintName("districts_province_code_fkey");
            });

            modelBuilder.Entity<GameType>(entity =>
            {
                entity.Property(e => e.GameTypeId).HasColumnName("GameTypeID");
            });

            modelBuilder.Entity<MatchOfTournament>(entity =>
            {
                entity.HasKey(e => e.MatchId)
                    .HasName("PK__MatchOfT__4218C837BE2ABD39");

                entity.Property(e => e.MatchId).HasColumnName("MatchID");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.MatchCode).HasMaxLength(50);

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.TableId).HasColumnName("TableID");

                entity.Property(e => e.TourId).HasColumnName("TourID");

                entity.HasOne(d => d.Table)
                    .WithMany(p => p.MatchOfTournaments)
                    .HasForeignKey(d => d.TableId)
                    .HasConstraintName("FK_MatchOfTournaments_Tables");

                entity.HasOne(d => d.Tour)
                    .WithMany(p => p.MatchOfTournaments)
                    .HasForeignKey(d => d.TourId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MatchOfTournaments_Tournaments");
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.HasIndex(e => e.AccId, "IX_News_AccID");

                entity.Property(e => e.NewsId).HasColumnName("NewsID");

                entity.Property(e => e.AccId).HasColumnName("AccID");

                entity.Property(e => e.Flyer).HasMaxLength(500);

                entity.Property(e => e.Link).HasMaxLength(255);

                entity.HasOne(d => d.Acc)
                    .WithMany(p => p.News)
                    .HasForeignKey(d => d.AccId);
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.Property(e => e.PlayerId).HasColumnName("PlayerID");

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.IsPayed).HasDefaultValueSql("((0))");

                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.Property(e => e.TourId).HasColumnName("TourID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Players)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_Players_Country");

                entity.HasOne(d => d.Tour)
                    .WithMany(p => p.Players)
                    .HasForeignKey(d => d.TourId)
                    .HasConstraintName("FK_Players_Tournaments");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Players)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Players_Users");
            });

            modelBuilder.Entity<PlayerInMatch>(entity =>
            {
                entity.HasKey(e => e.PlayerMatchId)
                    .HasName("PK__PlayerIn__0D8800C601C68548");

                entity.ToTable("PlayerInMatch");

                entity.Property(e => e.PlayerMatchId).HasColumnName("PlayerMatchID");

                entity.Property(e => e.GameWin).HasMaxLength(50);

                entity.Property(e => e.MatchId).HasColumnName("MatchID");

                entity.Property(e => e.PlayerId).HasColumnName("PlayerID");

                entity.HasOne(d => d.Match)
                    .WithMany(p => p.PlayerInMatches)
                    .HasForeignKey(d => d.MatchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlayerInMatch_MatchOfTournaments");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.PlayerInMatches)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlayerInMatch_Players");
            });

            modelBuilder.Entity<PlayerInSoloMatch>(entity =>
            {
                entity.HasKey(e => e.PlayerSoloMatchId);

                entity.ToTable("PlayerInSoloMatch");

                entity.Property(e => e.PlayerSoloMatchId).HasColumnName("PlayerSoloMatchID");

                entity.Property(e => e.GameWin).HasMaxLength(50);

                entity.Property(e => e.PlayerId).HasColumnName("PlayerID");

                entity.Property(e => e.SoloMatchId).HasColumnName("SoloMatchID");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.PlayerInSoloMatches)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlayerInSoloMatchs_Players");

                entity.HasOne(d => d.SoloMatch)
                    .WithMany(p => p.PlayerInSoloMatches)
                    .HasForeignKey(d => d.SoloMatchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlayerInSoloMatchs_SoloMatches");
            });

            modelBuilder.Entity<PlayerType>(entity =>
            {
                entity.Property(e => e.PlayerTypeId).HasColumnName("PlayerTypeID");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Title).HasMaxLength(50);
            });

            modelBuilder.Entity<Province>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("provinces_pkey");

                entity.ToTable("provinces");

                entity.HasIndex(e => e.AdministrativeRegionId, "idx_provinces_region");

                entity.HasIndex(e => e.AdministrativeUnitId, "idx_provinces_unit");

                entity.Property(e => e.Code)
                    .HasMaxLength(20)
                    .HasColumnName("code");

                entity.Property(e => e.AdministrativeRegionId).HasColumnName("administrative_region_id");

                entity.Property(e => e.AdministrativeUnitId).HasColumnName("administrative_unit_id");

                entity.Property(e => e.CodeName)
                    .HasMaxLength(255)
                    .HasColumnName("code_name");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.FullNameEn)
                    .HasMaxLength(255)
                    .HasColumnName("full_name_en");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(255)
                    .HasColumnName("name_en");

                entity.HasOne(d => d.AdministrativeRegion)
                    .WithMany(p => p.Provinces)
                    .HasForeignKey(d => d.AdministrativeRegionId)
                    .HasConstraintName("provinces_administrative_region_id_fkey");

                entity.HasOne(d => d.AdministrativeUnit)
                    .WithMany(p => p.Provinces)
                    .HasForeignKey(d => d.AdministrativeUnitId)
                    .HasConstraintName("provinces_administrative_unit_id_fkey");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).HasColumnName("RoleID");
            });

            modelBuilder.Entity<SoloMatch>(entity =>
            {
                entity.HasIndex(e => e.ClubId, "IX_SoloMatches_ClubID");

                entity.HasIndex(e => e.GameTypeId, "IX_SoloMatches_GameTypeID");

                entity.Property(e => e.SoloMatchId).HasColumnName("SoloMatchID");

                entity.Property(e => e.ClubId).HasColumnName("ClubID");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.Flyer).HasMaxLength(500);

                entity.Property(e => e.GameTypeId).HasColumnName("GameTypeID");

                entity.HasOne(d => d.Club)
                    .WithMany(p => p.SoloMatches)
                    .HasForeignKey(d => d.ClubId);

                entity.HasOne(d => d.GameType)
                    .WithMany(p => p.SoloMatches)
                    .HasForeignKey(d => d.GameTypeId);
            });

            modelBuilder.Entity<Table>(entity =>
            {
                entity.Property(e => e.ClubId).HasColumnName("ClubID");

                entity.Property(e => e.Image).HasMaxLength(500);

                entity.Property(e => e.Size)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TagName).HasMaxLength(255);

                entity.HasOne(d => d.Club)
                    .WithMany(p => p.Tables)
                    .HasForeignKey(d => d.ClubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tables_Clubs");
            });

            modelBuilder.Entity<Tournament>(entity =>
            {
                entity.HasKey(e => e.TourId)
                    .HasName("PK__Tourname__604CEA10E6A3F0D2");

                entity.Property(e => e.TourId).HasColumnName("TourID");

                entity.Property(e => e.ClubId).HasColumnName("ClubID");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Flyer).HasMaxLength(500);

                entity.Property(e => e.GameTypeId).HasColumnName("GameTypeID");

                entity.Property(e => e.PlayerTypeId).HasColumnName("PlayerTypeID");

                entity.Property(e => e.RaceToString).HasMaxLength(255);

                entity.Property(e => e.RegistrationDeadline).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.TourName).HasMaxLength(255);

                entity.Property(e => e.TournamentTypeId).HasColumnName("TournamentTypeID");

                entity.HasOne(d => d.Club)
                    .WithMany(p => p.Tournaments)
                    .HasForeignKey(d => d.ClubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tournaments_Clubs");

                entity.HasOne(d => d.GameType)
                    .WithMany(p => p.Tournaments)
                    .HasForeignKey(d => d.GameTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tournaments_GameTypes");

                entity.HasOne(d => d.PlayerType)
                    .WithMany(p => p.Tournaments)
                    .HasForeignKey(d => d.PlayerTypeId)
                    .HasConstraintName("FK_Tournaments_PlayerTypes");

                entity.HasOne(d => d.TournamentType)
                    .WithMany(p => p.Tournaments)
                    .HasForeignKey(d => d.TournamentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tournaments_TournamentTypes");
            });

            modelBuilder.Entity<TournamentType>(entity =>
            {
                entity.Property(e => e.TournamentTypeId).HasColumnName("TournamentTypeID");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Avatar).HasMaxLength(300);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");

                entity.Property(e => e.FullName).HasMaxLength(255);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.WardCode)
                    .HasMaxLength(20)
                    .HasColumnName("Ward_code");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_Accounts");

                entity.HasOne(d => d.WardCodeNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.WardCode)
                    .HasConstraintName("FK_Users_wards");
            });

            modelBuilder.Entity<Ward>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("wards_pkey");

                entity.ToTable("wards");

                entity.HasIndex(e => e.DistrictCode, "idx_wards_district");

                entity.HasIndex(e => e.AdministrativeUnitId, "idx_wards_unit");

                entity.Property(e => e.Code)
                    .HasMaxLength(20)
                    .HasColumnName("code");

                entity.Property(e => e.AdministrativeUnitId).HasColumnName("administrative_unit_id");

                entity.Property(e => e.CodeName)
                    .HasMaxLength(255)
                    .HasColumnName("code_name");

                entity.Property(e => e.DistrictCode)
                    .HasMaxLength(20)
                    .HasColumnName("district_code");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.FullNameEn)
                    .HasMaxLength(255)
                    .HasColumnName("full_name_en");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(255)
                    .HasColumnName("name_en");

                entity.HasOne(d => d.AdministrativeUnit)
                    .WithMany(p => p.Wards)
                    .HasForeignKey(d => d.AdministrativeUnitId)
                    .HasConstraintName("wards_administrative_unit_id_fkey");

                entity.HasOne(d => d.DistrictCodeNavigation)
                    .WithMany(p => p.Wards)
                    .HasForeignKey(d => d.DistrictCode)
                    .HasConstraintName("wards_district_code_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
