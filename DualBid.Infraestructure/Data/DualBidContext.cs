using System;
using System.Collections.Generic;
using DualBid.Infraestructure.Models;
using Microsoft.EntityFrameworkCore;

namespace DualBid.Infraestructure.Data;

public partial class DualBidContext : DbContext
{
    public DualBidContext(DbContextOptions<DualBidContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auction> Auction { get; set; }

    public virtual DbSet<Bid> Bid { get; set; }

    public virtual DbSet<Category> Category { get; set; }

    public virtual DbSet<Comic> Comic { get; set; }

    public virtual DbSet<Condition> Condition { get; set; }

    public virtual DbSet<ImgComic> ImgComic { get; set; }

    public virtual DbSet<ImgComic1> ImgComic1 { get; set; }

    public virtual DbSet<Publisher> Publisher { get; set; }

    public virtual DbSet<Role> Role { get; set; }

    public virtual DbSet<StateConservation> StateConservation { get; set; }

    public virtual DbSet<User> User { get; set; }

    public virtual DbSet<UserStatus> UserStatus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Subasta");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualEndDate).HasColumnName("actual_end_date");
            entity.Property(e => e.BasePrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("base_price");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.CreatorUserId).HasColumnName("creator_user_id");
            entity.Property(e => e.ExpectedEndDate).HasColumnName("expected_end_date");
            entity.Property(e => e.MinimunIncrease)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("minimun_increase");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.WinningBidId).HasColumnName("winning_bid_id");

            entity.HasOne(d => d.Comic).WithMany(p => p.Auction)
                .HasForeignKey(d => d.ComicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auction_Comic");

            entity.HasOne(d => d.CreatorUser).WithMany(p => p.Auction)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auction_User");

            entity.HasOne(d => d.WinningBid).WithMany(p => p.Auction)
                .HasForeignKey(d => d.WinningBidId)
                .HasConstraintName("FK_Auction_Bid");
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Puja");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AmountOffered)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount_offered");
            entity.Property(e => e.AuctionId).HasColumnName("auction_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.AuctionNavigation).WithMany(p => p.Bid)
                .HasForeignKey(d => d.AuctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bid_Auction");

            entity.HasOne(d => d.User).WithMany(p => p.Bid)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bid_User");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Categoria");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Comic>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConditionId).HasColumnName("condition_id");
            entity.Property(e => e.CreationDate).HasColumnName("creation_date");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.EditionNumber).HasColumnName("edition_number");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("ISBN");
            entity.Property(e => e.PublisherId).HasColumnName("publisher_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.StateConservationId).HasColumnName("state_conservation_id");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.YearPublication).HasColumnName("year_publication");

            entity.HasOne(d => d.Condition).WithMany(p => p.Comic)
                .HasForeignKey(d => d.ConditionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comic_Condition");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Comic)
                .HasForeignKey(d => d.PublisherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comic_Publisher");

            entity.HasOne(d => d.Seller).WithMany(p => p.Comic)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comic_User");

            entity.HasOne(d => d.StateConservation).WithMany(p => p.Comic)
                .HasForeignKey(d => d.StateConservationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comic_State_Conservation");

            entity.HasMany(d => d.Category).WithMany(p => p.Comic)
                .UsingEntity<Dictionary<string, object>>(
                    "ComicCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Comic_Category_Category"),
                    l => l.HasOne<Comic>().WithMany()
                        .HasForeignKey("ComicId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Comic_Category_Comic"),
                    j =>
                    {
                        j.HasKey("ComicId", "CategoryId").HasName("PK_Comic_Categoria");
                        j.ToTable("Comic_Category");
                        j.IndexerProperty<int>("ComicId").HasColumnName("comic_id");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                    });
        });

        modelBuilder.Entity<Condition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Condicion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        });

        modelBuilder.Entity<ImgComic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Imagen_comic");

            entity.ToTable("Img_ comic");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.Img).HasColumnName("img");

            entity.HasOne(d => d.Comic).WithMany(p => p.ImgComic)
                .HasForeignKey(d => d.ComicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Img_ comic_Comic");
        });

        modelBuilder.Entity<ImgComic1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Imagen_ comic");

            entity.ToTable("Img_comic");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComicId).HasColumnName("comic_id");
            entity.Property(e => e.Img).HasColumnName("img");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Editorial");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Rol");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        });

        modelBuilder.Entity<StateConservation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Estado_conservacion");

            entity.ToTable("State_Conservation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.LastNames)
                .HasMaxLength(100)
                .HasColumnName("last_names");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Role).WithMany(p => p.User)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.User)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_User_Status");
        });

        modelBuilder.Entity<UserStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Estado");

            entity.ToTable("User_Status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
