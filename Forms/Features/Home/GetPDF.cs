using BLG.AspNetCore;
using Forms.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public class GetPDF
    {
        public class Query : IRequest<Model>
        {
            public int ID { get; set; }
            public string FileName { get; set; }
        }

        public class Model
        {
            public string FileName { get; set; }
            public byte[] FileContents { get; set; }
            public string ContentType
            {
                get
                {
                    return "application/pdf";
                }
            }
        }

        public class QueryHandler : AsyncRequestHandler<Query, Model>
        {
            private readonly IDbContext _context;
            private readonly IP8ServicesV1Proxy _proxy;

            public QueryHandler(IDbContext context, IP8ServicesV1Proxy proxy)
            {
                _context = context;
                _proxy = proxy;
            }

            protected async override Task<Model> HandleCore(Query request)
            {
                var fileNetDocID = await GetFileNetDocIDAsync(request.ID);
                var client = await _proxy.GetClientAsync();
                var document = await client.getDocumentByIDAsync(string.Empty, string.Empty, Constants.FileNetDocClass, fileNetDocID.ToString(), true, null);
                var documentContent = document.contentList.FirstOrDefault();

                var model = new Model
                {
                    FileName = documentContent.fileName,
                    FileContents = documentContent.contentMTOM
                };

                return model;
            }

            private async Task<Guid> GetFileNetDocIDAsync(int ID)
            {
                var sql = @"SELECT FileNetDocID FROM frm.Forms WHERE FormID = @ID";
                var query = await _context.QueryAsync<Guid>(sql, new { ID = ID });
                return query.FirstOrDefault();
            }
        }
    }
}
