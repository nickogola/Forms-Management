using ClosedXML.Excel;
using Forms.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public class Export
    {
        public class Query : IRequest<Model>
        { }

        public class Model
        {
            public string FileName { get; set; }
            public byte[] FileContents { get; set; }
            public string ContentType
            {
                get
                {
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
            }
        }

        public class ExceptionPage
        {
            public ExceptionPage()
            {
                StatePrograms = new List<ExStateProgram>();
            }

            public int ID { get; set; }
            public string Title { get; set; }
            public string Company { get; set; }
            public string LineOfBusiness { get; set; }
            public string FileName { get; set; }
            public IList<ExStateProgram> StatePrograms { get; set; }

            public string DisplayStateProgram
            {
                get
                {
                    var statePrograms =
                        from stateProgram in StatePrograms
                        orderby stateProgram.StateCode, stateProgram.ProgramCode
                        select new
                        {
                            DisplayStateCode = string.Format("{0}-{1}", stateProgram.StateCode, stateProgram.ProgramCode)
                        };

                    return string.Join(",", statePrograms.Select(x => x.DisplayStateCode));
                }
            }
        }

        public class ExStateProgram
        {
            public int ExceptionPageID { get; set; }
            public string StateCode { get; set; }
            public string ProgramCode { get; set; }
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
                var exceptionPages = await GetExceptionPagesAsync();
                var stream = CreateXLWorkbookStream(exceptionPages);

                var result = new Model
                {
                    FileName = "Forms.xlsx",
                    FileContents = stream.ToArray()
                };

                return result;
            }

            private async Task<IList<ExceptionPage>> GetExceptionPagesAsync()
            {
                var sql = @"
                    SELECT p.ID, p.Title, c.[Name] as Company, l.[Name] as LineOfBusiness, p.FileName
                    FROM expg.ExceptionPage p
                    INNER JOIN expg.Company c ON p.CompanyID = c.ID
                    INNER JOIN expg.LineOfBusiness l ON p.LineOfBusinessID = l.ID

                    SELECT psp.ExceptionPageID, fsp.StateCode, fsp.ProgramCode
                    FROM expg.ExceptionPage p
                    LEFT OUTER JOIN expg.ExceptionPageStateProgram psp ON p.ID = psp.ExceptionPageID
                    LEFT OUTER JOIN frm.StateProgram fsp ON psp.StateProgramID = fsp.StateProgramID";

                var multi = await _context.QueryMultipleAsync(sql);
                var exceptionPages = multi.Read<ExceptionPage>().ToList();
                var exStateProgramsMap = multi.Read<ExStateProgram>()
                    .GroupBy(x => x.ExceptionPageID)
                    .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var exceptionPage in exceptionPages)
                {
                    List<ExStateProgram> statePrograms;
                    if (exStateProgramsMap.TryGetValue(exceptionPage.ID, out statePrograms))
                        exceptionPage.StatePrograms = statePrograms;
                }

                return exceptionPages;
            }

            private MemoryStream CreateXLWorkbookStream(IList<ExceptionPage> exceptionPages)
            {
                var stream = new MemoryStream();
                var workbook = new XLWorkbook();
                var worksheet = workbook.AddWorksheet("Summary");
                var headerRow = worksheet.Row(1);
                var headers = new List<string> { "ExceptionPageID", "Title", "StateProgram", "Company", "LineOfBusiness", "FileName" };
                int headerIndex = 1;

                foreach (var header in headers)
                {
                    headerRow.Cell(headerIndex).Value = header;
                    headerRow.Cell(headerIndex).Style.Font.Bold = true;
                    headerRow.Cell(headerIndex).Style.Font.Underline = XLFontUnderlineValues.Single;
                    headerIndex++;
                }

                int rowIndex = 2;

                foreach (var exceptionPage in exceptionPages)
                {
                    var row = worksheet.Row(rowIndex);

                    row.Cell(1).Value = exceptionPage.ID;
                    row.Cell(2).Value = exceptionPage.Title;
                    row.Cell(3).Value = exceptionPage.DisplayStateProgram;
                    row.Cell(4).Value = exceptionPage.Company;
                    row.Cell(5).Value = exceptionPage.LineOfBusiness;

                    rowIndex++;
                }

                worksheet.Columns(1, 5).AdjustToContents();
                workbook.SaveAs(stream);

                return stream;
            }
        }
    }
}
