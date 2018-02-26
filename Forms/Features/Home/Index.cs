using Dapper;
using Forms.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public class Index
    {
        public class Query : IRequest<Model>
        {
            public string FormName { get; set; }
            public string FormNumber { get; set; }
          //  public int? CompanyID { get; set; }
            public int? StateProgramID { get; set; }
            public int? LineOfBusinessID { get; set; }
            public string ActiveRulesOnly { get; set; }
        }

        public class Model
        {
            public Model()
            {
                //Companies = new List<SelectListItem>();
                StatePrograms = new List<SelectListItem>();
                LinesOfBusiness = new List<SelectListItem>();
                Forms = new List<Form>();
               // ExceptionPages = new List<ExceptionPage>();
            }

            public string FormNumber { get; set; }
            public string FormName { get; set; }
            public int? StateProgramID { get; set; }
            public int? LineOfBusinessID { get; set; }
            public string ActiveRulesOnly { get; set; }
            public IList<SelectListItem> StatePrograms { get; set; }
            public IList<StateProgram> FormStatePrograms { get; set; }
            public IList<SelectListItem> LinesOfBusiness { get; set; }
            public IList<Form> Forms { get; set; }
        }

       

        public class Form
        {
            public Form()
            {
               FormStatePrograms = new List<StateProgram>();
            }

            public int FormID { get; set; }
            public int FormCategoryID { get; set; }
            public string FormName { get; set; }
            public string FormNumber { get; set; }
            //public string FileName { get; set; }
            public string FormDocumentPath { get; set; }
          //  public DateTime UpdateDT { get; set; }
            public string State_Program { get; set; }
            public IList<StateProgram> FormStatePrograms { get; set; }

            public string DisplayStateProgram
            {
                get
                {
                    var statePrograms =
                        from stateProgram in FormStatePrograms
                        orderby stateProgram.StateCode, stateProgram.ProgramCode
                        select new
                        {
                            DisplayStateCode = string.Format("{0}-{1}", stateProgram.StateCode, stateProgram.ProgramCode)
                        };

                    return string.Join(",", statePrograms.Select(x => x.DisplayStateCode));
                }
            }
        }
        public class StateProgram
        {
            public int FormID { get; set; }
            public string StateCode { get; set; }
            public string ProgramCode { get; set; }
        }

        public class QueryHandler : AsyncRequestHandler<Query, Model>
        {
            private readonly IDbContext _context;
            private readonly ISelectListItemBuilder _builder;

            public QueryHandler(IDbContext context, ISelectListItemBuilder builder)
            {
                _context = context;
                _builder = builder;
            }

            protected async override Task<Model> HandleCore(Query request)
            {
                var model = new Model
                {
                    StateProgramID = request.StateProgramID,
                    LineOfBusinessID = request.LineOfBusinessID,
                    ActiveRulesOnly = request.ActiveRulesOnly,
                    StatePrograms = await _builder.BuildStateProgramSelectListItemsAsync(),
                    LinesOfBusiness = await _builder.BuildLineOfBusinessSelectListItemsAsync(),
                    Forms = await MapFormsAsync(request)
                };

                return model;
            }

            private async Task<IList<Form>> MapFormsAsync(Query request)
            {
                var forms = await GetFormsAsync(request);
                var exStatePrograms = await GetExStateProgramsAsync(request);

                foreach (var form in forms)
                {
                    List<StateProgram> statePrograms;
                    if (exStatePrograms.TryGetValue(form.FormID, out statePrograms))
                        form.FormStatePrograms = statePrograms;
                }

                return forms;
            }
      
            private async Task<IList<Form>> GetFormsAsync(Query request)
            {
                var builder = new SqlBuilder();
                var template = builder.AddTemplate(@"SELECT DISTINCT   f.FormID
		                                , f.FormCategoryID
		                                , f.FormName
		                                , f.FormNumber
		                                , '' AS State_Program
		                                , f.FormDocumentPath

                                FROM frm.Forms f

                                LEFT JOIN frm.FormCategory fc
                                ON f.FormCategoryID = FC.FormCategoryID

                                LEFT JOIN frm.Forms_StateProgram fsp
                                ON fsp.FormID = f.FormID

                                left JOIN frm.StateProgram_LineOfBusiness spl
                                ON spl.FormID = fsp.FormID and fsp.StateProgramID = spl.StateProgramID
                                    /**where**/
                                    /**orderby**/");


                //  ORDER BY f.FormCategoryID, F.FormNumber");

                if (!String.IsNullOrEmpty(request.ActiveRulesOnly))
                    builder.Where("(@Active_Forms_Only = 'N' OR fsp.EffectiveDate <= GETDATE()) AND (@Active_Forms_Only = 'N' OR ISNULL(fsp.ExpirationDate,'2099-12-31') >= GETDATE())", new { Active_Forms_Only = request.ActiveRulesOnly });

                if (!String.IsNullOrEmpty(request.FormName))
                    builder.Where(
                        "(f.FormName LIKE '%' + @Form_Name + '%'  OR @Form_Name = '')",
                        new { Form_Name = request.FormName }
                    );

                if (!String.IsNullOrEmpty(request.FormNumber))
                    builder.Where("(f.FormNumber LIKE '%' + @Form_Number + '%' OR @Form_Number = '')", new { Form_Number = request.FormNumber });

                if (request.StateProgramID != null)
                    builder.Where(
                        "(fsp.StateProgramID = @StateProgramID OR @StateProgramID is NULL)",
                        new { StateProgramID = request.StateProgramID }
                    );
                if (request.LineOfBusinessID != null)
                    builder.Where(
                        "(spl.LineOfBusinessID = @LobID OR @LobID is NULL)",
                        new { LobID = request.LineOfBusinessID }
                    );
                builder.OrderBy("f.FormCategoryID, F.FormNumber");

                var query = await _context.QueryAsync<Form>(template.RawSql, template.Parameters);
                return query.ToList();
            }

            private async Task<Dictionary<int, List<StateProgram>>> GetExStateProgramsAsync(Query request)
            {
                var builder = new SqlBuilder();
                var template = builder.AddTemplate(@"
                SELECT d.FormID, d.FormNumber, d.FormName, d.StateCode, d.ProgramCode
                FROM (	SELECT f.FormID, f.FormNumber, f.FormName, sp.StateCode, sp.ProgramCode, fsp.EffectiveDate, ISNULL(fsp.ExpirationDate, '2099-12-31') as ExpirationDate, spl.LineOfBusinessID
		                FROM frm.Forms f
		                LEFT OUTER JOIN frm.Forms_StateProgram fsp ON f.FormID = fsp.FormID
		                LEFT OUTER JOIN frm.StateProgram sp ON fsp.StateProgramID = sp.StateProgramID
                        LEFT OUTER JOIN frm.StateProgram_LineOfBusiness spl ON spl.FormID = fsp.FormID and fsp.StateProgramID = spl.StateProgramID
	                ) d
                /**where**/");

                if (request.FormName != null)
                    builder.Where("d.FormName = @FormName", new { FormName = request.FormName });

                if (request.FormNumber != null)
                    builder.Where("d.FormNumber = @FormNumber", new { FormNumber = request.FormNumber });

                if (request.LineOfBusinessID != null)
                    builder.Where("d.LineOfBusinessID = @LineOfBusinessID", new { LineOfBusinessID = request.LineOfBusinessID });

                if (!string.IsNullOrEmpty(request.ActiveRulesOnly))
                    builder.Where("d.EffectiveDate <= @CurrentDate AND @CurrentDate <= d.ExpirationDate", new { CurrentDate = DateTime.Now.ToString("yyyy-MM-dd") });

                var query = await _context.QueryAsync<StateProgram>(template.RawSql, template.Parameters);

                return query
                    .GroupBy(x => x.FormID)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }
        }
    }
}
