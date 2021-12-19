using AutoMapper;
using CinigazStokEntity;
using CinigazStokService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinigazStokService.Models.Item;
using CinigazStokService.Models.Category;

namespace CinigazStokService.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryModel>();

            CreateMap<Brand, BrandModel>();

            CreateMap<Item, ItemModel>()
                .ForMember(v => v.requiredFieldValues, opt =>
                    opt.MapFrom(vr => string.IsNullOrEmpty(vr.RequiredFieldValues) ? new List<RequiredFieldValues>() : JsonConvert.DeserializeObject<List<RequiredFieldValues>>(vr.RequiredFieldValues)))
                .ForMember(v => v.specifications, opt =>
                    opt.MapFrom(vr => string.IsNullOrEmpty(vr.Specifications) ? new List<Specification>() : JsonConvert.DeserializeObject<List<Specification>>(vr.Specifications)))
                .ForMember(v => v.historyRecords, opt => opt.Ignore())
                .ForMember(v => v.variants, opt => opt.Ignore())
                .ForMember(v => v.Category, opt => opt.MapFrom(v => v.Category));

            CreateMap<StockHistory, StockHistoryModel>()
                .ForMember(v => v.RequiredFields, opt =>
                    opt.MapFrom(vr => string.IsNullOrEmpty(vr.RequiredFields) ? new List<RequiredField>() : JsonConvert.DeserializeObject<List<RequiredField>>(vr.RequiredFields)))

                .ForMember(v => v.FromLocation,
                    opt => opt.MapFrom(vr => vr.FromLocation.Name + (vr.FromLocation.Company != null ? " (" + vr.FromLocation.Company.Name + ")" : "")))

                .ForMember(v => v.ToLocation,
                    opt => opt.MapFrom(vr =>vr.ToLocation.Name + (vr.ToLocation.Company != null ? " (" + vr.ToLocation.Company.Name + ")" : "")))

                .ForMember(v => v.categoryName,
                    opt => opt.MapFrom(vr => vr.Item.Category.Name))

                .ForMember(v => v.TransType,
                    opt => opt.MapFrom(vr => vr.TransType.Name))

                .ForMember(v => v.Brand,
                    opt => opt.MapFrom(vr => vr.Item.Brand))

                .ForMember(v => v.BrandId,
                    opt => opt.MapFrom(vr => vr.Item.BrandId))

                .ForMember(v => v.SerialNumber,
                    opt => opt.MapFrom(vr => vr.Item.SerialNumber))

                .ForMember(v => v.categoryName,
                    opt => opt.MapFrom(vr => vr.Item.Category.Name))

                .ForMember(v => v.ItemName,
                    opt => opt.MapFrom(vr => vr.Item.Name))

                 .ForMember(v => v.ItemRequiredFields,
                    opt => opt.MapFrom(vr => string.IsNullOrEmpty(vr.Item.RequiredFieldValues) ? new List<RequiredFieldValues>()  : JsonConvert.DeserializeObject<List<RequiredFieldValues>>(vr.Item.RequiredFieldValues)))

                .ForMember(v=>v.VariantParams,
                    opt => opt.MapFrom(vr => vr.Variant.VariantParams == null ? new List<VariantParams>() : JsonConvert.DeserializeObject<List<VariantParams>>(vr.Variant.VariantParams)));

        }
    }
}
