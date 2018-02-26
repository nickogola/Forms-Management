using Forms.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public interface ISelectListItemBuilder
    {
      //  Task<IList<SelectListItem>> BuildCompanySelectListItemsAsync();
        Task<IList<SelectListItem>> BuildStateProgramSelectListItemsAsync();
        Task<IList<SelectListItem>> BuildLineOfBusinessSelectListItemsAsync();
        Task<IList<SelectListItem>> BuildFormCategoriesSelectListItemsAsync();
        Task<IList<SelectListItem>> BuildFormTypesSelectListItemsAsync();
    }
    public class SelectListItemBuilder : ISelectListItemBuilder
    {
        private readonly IDbContext _context;

        public SelectListItemBuilder(IDbContext context)
        {
            _context = context;
        }

        public async Task<IList<SelectListItem>> BuildFormCategoriesSelectListItemsAsync()
        {
            var sql = @"SELECT FormCategoryID, FormCategory as [FormCategoryName] FROM frm.FormCategory";
            var formCategories = await _context.QueryAsync<FormCategory>(sql);
            var list = new List<SelectListItem>();

            foreach (var category in formCategories)
            {
                list.Add(new SelectListItem
                {
                    Text = category.FormCategoryName,
                    Value = category.FormCategoryID.ToString()
                });
            }

            return list;
        }
       
        public async Task<IList<SelectListItem>> BuildStateProgramSelectListItemsAsync()
        {
            var sql = @"SELECT StateProgramID, StateCode, ProgramCode FROM frm.StateProgram";
            var statePrograms = await _context.QueryAsync<StateProgram>(sql);

            var list =
                from stateProgram in statePrograms
                orderby stateProgram.StateCode, stateProgram.ProgramCode
                select new SelectListItem
                {
                    Text = $"{stateProgram.StateCode}-{stateProgram.ProgramCode}",
                    Value = stateProgram.StateProgramID.ToString()
                };

            return list.ToList();
        }

        public async Task<IList<SelectListItem>> BuildLineOfBusinessSelectListItemsAsync()
        {
            var sql = @"SELECT LineOfBusinessID as ID, LineOfBusiness as [Name] FROM frm.LineOfBusiness";
            var linesOfBusiness = await _context.QueryAsync<LineOfBusiness>(sql);

            var list =
                from lineOfBusiness in linesOfBusiness
                select new SelectListItem
                {
                    Text = lineOfBusiness.Name,
                    Value = lineOfBusiness.ID.ToString()
                };

            return list.ToList();
        }

        public async Task<IList<SelectListItem>> BuildFormTypesSelectListItemsAsync()
        {
            var sql = @"SELECT FormTypeID, FormType as [FormTypeName] FROM frm.FormType";
            var formTypes = await _context.QueryAsync<FormType>(sql);
            var list = new List<SelectListItem>();

            foreach (var type in formTypes)
            {
                list.Add(new SelectListItem
                {
                    Text = type.FormTypeName,
                    Value = type.FormTypeID.ToString()
                });
            }

            return list;
        }
        public class FormCategory
        {
            public int FormCategoryID { get; set; }
            public string FormCategoryName { get; set; }
        }
        public class FormType
        {
            public int FormTypeID { get; set; }
            public string FormTypeName { get; set; }
        }

        public class StateProgram
        {
            public int StateProgramID { get; set; }
            public string StateCode { get; set; }
            public string ProgramCode { get; set; }
        }

        public class LineOfBusiness
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class StateProgram_LineOfBusiness
        {
            public int FormID { get; set; }
            public int LineOfBusinessID { get; set; }
            public int StateProgramID { get; set; }
        }
    }
}

