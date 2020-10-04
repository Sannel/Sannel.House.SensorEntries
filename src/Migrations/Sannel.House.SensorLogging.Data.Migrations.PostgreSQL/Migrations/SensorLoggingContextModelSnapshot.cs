﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Sannel.House.SensorLogging.Data;

namespace Sannel.House.SensorLogging.Data.Migrations.PostgreSQL.Migrations
{
    [DbContext(typeof(SensorLoggingContext))]
    partial class SensorLoggingContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Sannel.House.SensorLogging.Models.Device", b =>
                {
                    b.Property<Guid>("LocalDeviceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int?>("DeviceId")
                        .HasColumnType("integer");

                    b.Property<long?>("MacAddress")
                        .HasColumnType("bigint");

                    b.Property<string>("Manufacture")
                        .HasColumnType("text");

                    b.Property<string>("ManufactureId")
                        .HasColumnType("text");

                    b.Property<Guid?>("Uuid")
                        .HasColumnType("uuid");

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
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("LocalDeviceId")
                        .HasColumnType("uuid");

                    b.Property<int>("SensorType")
                        .HasColumnType("integer");

                    b.HasKey("SensorEntryId");

                    b.HasIndex("LocalDeviceId");

                    b.HasIndex("SensorType");

                    b.ToTable("SensorEntries");
                });

            modelBuilder.Entity("Sannel.House.SensorLogging.Models.SensorReading", b =>
                {
                    b.Property<Guid?>("SensorReadingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("character varying(256)")
                        .HasMaxLength(256);

                    b.Property<Guid>("SensorEntryId")
                        .HasColumnType("uuid");

                    b.Property<double>("Value")
                        .HasColumnType("double precision");

                    b.HasKey("SensorReadingId");

                    b.HasIndex("Name");

                    b.HasIndex("SensorEntryId");

                    b.ToTable("SensorReadings");
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
