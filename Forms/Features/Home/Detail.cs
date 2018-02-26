using Forms.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public class Detail
    {
        public class Query : IRequest<Model>
        {
            public int ID { get; set; }
        }

        public class Model
        {
            public Model()
            {
                StatePrograms = new List<StateProgram>();
            }

            public int FormID { get; set; }
            public string FormName { get; set; }
            public string FormNumber { get; set; }
            public string FormDocumentPath { get; set; }
            public int FormCategoryID { get; set; }

            public IList<StateProgram> StatePrograms { get; set; }
        }

        public class StateProgram
        {
            public int FormID { get; set; }
            public int StateProgramID { get; set; }
            public string StateCode { get; set; }
            public string ProgramCode { get; set; }
            public DateTime EffectiveDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
        }

        public class QueryHandler : AsyncRequestHandler<Query, Model>
        {
            private readonly IDbContext _context;

            public QueryHandler(IDbContext context)
            {
                _context = context;
            }

            protected async override Task<Model> HandleCore(Query request)
            {
                var sql = @"
                        SELECT f.FormID, f.FormName, f.FormNumber, f.FormDocumentPath, f.FormCategoryID
                        FROM frm.Forms f
                     
                        WHERE f.FormID = @FormID

                        SELECT fm.FormID, fm.StateProgramID, fsp.StateCode, fsp.ProgramCode, fm.EffectiveDate, fm.ExpirationDate
                        FROM frm.Forms_StateProgram fm
                        INNER JOIN frm.StateProgram fsp ON fm.StateProgramID = fsp.StateProgramID
                        WHERE fm.FormID = @FormID";

                var multi = await _context.QueryMultipleAsync(sql, new { FormID = request.ID });
                var form = multi.Read<Model>().FirstOrDefault();
                var statePrograms = multi.Read<StateProgram>().ToList();

                if (form != null)
                    form.StatePrograms = statePrograms;

                return form;
            }
        }
    }
}
