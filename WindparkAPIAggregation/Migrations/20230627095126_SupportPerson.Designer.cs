﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WindParkAPIAggregation.Repository;

#nullable disable

namespace WindParkAPIAggregation.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230627095126_SupportPerson")]
    partial class SupportPerson
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WindParkAPIAggregation.Contracts.Models.SupportPerson", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TurbineId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("TurbineId")
                        .IsUnique();

                    b.ToTable("SupportPerson");
                });

            modelBuilder.Entity("WindParkAPIAggregation.Contracts.Models.Turbine", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("CurrentProduction")
                        .HasColumnType("float");

                    b.Property<int>("TurbineNumber")
                        .HasColumnType("int");

                    b.Property<Guid>("WindParkId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("WindSpeed")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("WindParkId");

                    b.ToTable("Turbine");
                });

            modelBuilder.Entity("WindParkAPIAggregation.Contracts.Models.WindPark", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("WindParkId");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<int>("WindParkNumber")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("WindPark");
                });

            modelBuilder.Entity("WindParkAPIAggregation.Contracts.Models.SupportPerson", b =>
                {
                    b.HasOne("WindParkAPIAggregation.Contracts.Models.Turbine", "Turbine")
                        .WithOne("SupportPerson")
                        .HasForeignKey("WindParkAPIAggregation.Contracts.Models.SupportPerson", "TurbineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Turbine");
                });

            modelBuilder.Entity("WindParkAPIAggregation.Contracts.Models.Turbine", b =>
                {
                    b.HasOne("WindParkAPIAggregation.Contracts.Models.WindPark", "WindPark")
                        .WithMany("Turbines")
                        .HasForeignKey("WindParkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("WindPark");
                });

            modelBuilder.Entity("WindParkAPIAggregation.Contracts.Models.Turbine", b =>
                {
                    b.Navigation("SupportPerson");
                });

            modelBuilder.Entity("WindParkAPIAggregation.Contracts.Models.WindPark", b =>
                {
                    b.Navigation("Turbines");
                });
#pragma warning restore 612, 618
        }
    }
}
