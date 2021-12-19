using CinigazStokEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinigazStokService.Models;
using CinigazStokService.Models.Item;

namespace CinigazStokService.services
{

    public interface ICatagoryValidation
    {
        Result<Item> EnsureMandotaryCatagoryFields(ItemModel request);
    }


    public class CatagoryValidation: ICatagoryValidation
    {
        public readonly StokDbContext context;
        public CatagoryValidation(StokDbContext context)
        {
            this.context = context;
        }

        public Result<Item> EnsureMandotaryCatagoryFields(ItemModel request)
        {
            var Return = new Result<Item>();
            Return.Meta = new Meta();
            Return.Meta.IsSuccess = true;
            var NewCategory = context.Categories.FirstOrDefault(c => c.Id == request.categoryId);
            if (NewCategory == null)
            {
                Return.Meta.IsSuccess = false;
                Return.Meta.Error = "category.not.found";
                Return.Meta.ErrorMessage = "Kategori bulunamadı!";
                return Return;
            }

            var Fields = context.CategoryFields.Where(c => c.CategoryId == NewCategory.Id);
            if (Fields != null)
            {
                var requiredFields = Fields.Where(f => f.IsMandatory).Select(r => r.Name).ToList();

                var emptyFields = request.requiredFieldValues.Where(r => string.IsNullOrEmpty(r.value));

                if (emptyFields.Any())
                {

                    var missingFields = emptyFields.Where(x => requiredFields.Contains(x.name));

                    if (missingFields.Any())
                    {
                        Return.Meta.IsSuccess = false;
                        Return.Meta.Error = "missing.catagory.requiredfield";
                        Return.Meta.ErrorMessage = ($"Katagori zorunlu alanlari eksiktir!" + missingFields);
                        return Return;

                    }
                }

            }

            return Return;
        }
    }
}
