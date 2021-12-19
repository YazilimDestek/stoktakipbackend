using System;
using System.Collections.Generic;
using System.Text;

namespace CinigazStokEntity.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Text;

    namespace CinigazStokEntity.Entity.Persistence
    {
        public static class StokDbInitializer
        {
            public static void Initialize(StokDbContext context)
            {
                try
                {
                    //context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    context.Database.Migrate();

                    if (context.Companies.ToList().Count() == 0)
                    {
                        // FİRMALAR
                        #region companies
                        var companies = new Company[]
                        {
                            new Company {Name = "Çinigaz", IsDeleted = false},
                            new Company {Name = "Diğer", IsDeleted = false}
                        };

                        foreach (var item in companies)
                        {
                            context.Companies.Add(item);
                        }

                        context.SaveChanges();
                        #endregion

                        // LOKASYON TİPLERİ
                        #region locationtypes
                        var locationTypes = new LocationType[]
                        {
                            new LocationType{ Name = "Sabit" },
                            new LocationType{ Name = "Seyyar"}
                        };

                        foreach (var item in locationTypes)
                        {
                            context.LocationTypes.Add(item);
                        }

                        context.SaveChanges();
                        #endregion

                        // LOKASYONLAR
                        #region locations
                        var locations = new Location[]
                        {
                            new Location{ Name = "Firma", IsDeleted = false, LocationTypeId = locationTypes[0].Id, CompanyId = companies[1].Id },
                            new Location{ Name = "Müşteri", IsDeleted = false, LocationTypeId = locationTypes[0].Id, CompanyId = companies[1].Id },
                            new Location{ Name = "Hurda", IsDeleted = false, LocationTypeId = locationTypes[0].Id, CompanyId = companies[0].Id },
                            new Location{ Name = "Gayip", IsDeleted = false, LocationTypeId = locationTypes[0].Id , CompanyId = companies[1].Id },
                            new Location{ Name = "Kütahya", IsDeleted = false, LocationTypeId = locationTypes[0].Id , CompanyId = companies[0].Id },
                            new Location{ Name = "Ankara", IsDeleted = false, LocationTypeId = locationTypes[0].Id , CompanyId = companies[1].Id },
                            new Location{ Name = "Gediz", IsDeleted = false, LocationTypeId = locationTypes[0].Id , CompanyId = companies[1].Id },
                            new Location{ Name = "Çavdarhisar", IsDeleted = false, LocationTypeId = locationTypes[0].Id , CompanyId = companies[1].Id },
                            new Location{ Name = "Simav", IsDeleted = false, LocationTypeId = locationTypes[0].Id , CompanyId = companies[1].Id },
                            new Location{ Name = "Tavşanlı", IsDeleted = false, LocationTypeId = locationTypes[0].Id , CompanyId = companies[1].Id },
                            new Location{ Name = "Servis Elemanları", IsDeleted = false, LocationTypeId = locationTypes[1].Id , CompanyId = companies[1].Id },
                            new Location{ Name = "Servis Firmaları", IsDeleted = false, LocationTypeId = locationTypes[1].Id , CompanyId = companies[1].Id },
                            new Location{ Name = "Kargo", IsDeleted = false, LocationTypeId = locationTypes[1].Id , CompanyId = companies[1].Id }
                        };

                        foreach (var item in locations)
                        {
                            context.Locations.Add(item);
                        }

                        context.SaveChanges();
                        #endregion

                        // STOK HAREKET TANIMLARI
                        #region transtypes
                        var transTypes = new TransType[]
                        {
                            new TransType{ Name = "Yeni", IsDeleted = false },
                            new TransType{ Name = "Sökme", IsDeleted = false },
                            new TransType{ Name = "Takma", IsDeleted = false },
                            new TransType{ Name = "İade", IsDeleted = false },
                            new TransType{ Name = "Rezervasyon", IsDeleted = false },
                            new TransType{ Name = "Düzeltme", IsDeleted = false },
                            new TransType{ Name = "Tamir & Bakım ve Kalibrasyon", IsDeleted = false },
                            new TransType{ Name = "Abys İşlemi", IsDeleted = false },
                            new TransType{ Name = "Gayip'e Çıkar", IsDeleted = false },
                            new TransType{ Name = "Gayip'ten Getir", IsDeleted = false }
                        };

                        foreach (var item in transTypes)
                        {
                            context.TransTypes.Add(item);
                        }

                        context.SaveChanges();
                        #endregion

                        // KULLANICILAR
                        #region users
                        var users = new User[]
                        {
                            new User{ Username = "firat@albatrosyazilim.com", IsAdmin = true, Password = CreateMD5("Frt159753"), CreatedDateTime = DateTime.Now, IsActive = true, IsDeleted = false },
                            new User{ Username = "demo", IsAdmin = true, Password = CreateMD5("123456"), CreatedDateTime = DateTime.Now, IsActive = true, IsDeleted = false }
                        };

                        foreach (var item in users)
                        {
                            context.Users.Add(item);
                        }

                        context.SaveChanges();
                        #endregion

                        // MARKALAR
                        #region brands
                        var brands = new Brand[]
                        {
                            new Brand{ Name = "Eca" },
                            new Brand{ Name = "Kale Kalıp" }
                        };

                        foreach (var item in brands)
                        {
                            context.Brands.Add(item);
                        }

                        context.SaveChanges();
                        #endregion

                        // KATEGORİLER
                        #region categories
                        var categories = new Category[]
                        {
                            new Category{ Name = "Sayaç" },
                            new Category{ Name = "Regülatör" },
                            new Category{ Name = "Modül" }
                        };

                        foreach (var item in categories)
                        {
                            context.Categories.Add(item);
                        }
                        #endregion

                        // TİPLER
                        #region itemTypes
                        var itemTypes = new ItemType[]
                        {
                            new ItemType{ Name = "G4" },
                            new ItemType{ Name = "G5" }
                        };

                        foreach (var item in itemTypes)
                        {
                            context.ItemTypes.Add(item);
                        }
                        #endregion itemTypes

                        // TÜRLER
                        #region itemKinds
                        var itemKinds = new ItemKind[]
                        {
                            new ItemKind{ Name = "Mekanik" },
                            new ItemKind{ Name = "Elektronik" }
                        };

                        foreach (var item in itemKinds)
                        {
                            context.ItemKinds.Add(item);
                        }
                        #endregion itemKinds

                        context.SaveChanges();

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("INITIALIZE EDERKEN HATA OLUŞTU!! " + e.Message);
                }
            }

            public static string CreateMD5(string input)
            {
                //Use input string to calculate MD5 hash
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
        }
    }

}
