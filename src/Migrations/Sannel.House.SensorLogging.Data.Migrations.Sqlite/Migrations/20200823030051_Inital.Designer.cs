﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sannel.House.SensorLogging.Data;

namespace Sannel.House.SensorLogging.Data.Migrations.Sqlite.Migrations
{
    [DbContext(typeof(SensorLoggingContext))]
    [Migration("20200823030051_Inital")]
    partial class Inital
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7");

            modelBuilder.Entity("Sannel.House.SensorLogging.Models.Device", b =>
                {
                    b.Property<Guid>("LocalDeviceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int?>("DeviceId")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("MacAddress")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Manufacture")
                        .HasColumnType("TEXT");

                    b.Property<string>("ManufactureId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("Uuid")
                        .HasColumnType("TEXT");

                    b.HasKey("LocalDeviceId");

                    b.HasIndex("DeviceId");

                    b.HasIndex("MacAddress");

                    b.HasIndex("Uuid");

                    b.HasIndex("Manufacture", "ManufactureId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Sannel.House.SensorLogging.Models.SensorEntry", b =>
                {
                    b.Property<Guid>("SensorEntryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("LocalDeviceId")
                        .HasColumnType("TEXT");

                    b.Property<int>("SensorType")
                        .HasColumnType("INTEGER");

                    b.HasKey("SensorEntryId");

                    b.HasIndex("LocalDeviceId");

                    b.HasIndex("SensorType");

                    b.ToTable("SensorEntries");
                });

            modelBuilder.Entity("Sannel.House.SensorLogging.Models.SensorReading", b =>
                {
                    b.Property<Guid?>("SensorReadingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<Guid>("SensorEntryId")
                        .HasColumnType("TEXT");

                    b.Property<double>("Value")
                        .HasColumnType("REAL");

                    b.HasKey("SensorReadingId");

                    b.HasIndex("Name");

                    b.HasIndex("SensorEntryId");

                    b.ToTable("SensorReading");
                });

            modelBuilder.Entity("Sannel.House.SensorLogging.Models.SensorReading", b =>
                {
                    b.HasOne("Sannel.House.SensorLogging.Models.SensorEntry", "SensorEntry")
                        .WithMany("Values")
                        .HasForeignKey("SensorEntryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}