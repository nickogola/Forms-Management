using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Forms.Filters;

namespace Forms.Features.Home
{
   // [Authorize(Policy = Constants.DefaultSecurityPolicy)]
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(Index.Query query)
        {
            var model = await _mediator.Send(query);
            return View(model);
        }

        public async Task<IActionResult> Detail(Detail.Query query)
        {
            var model = await _mediator.Send(query);
            return View(model);
        }

        public async Task<IActionResult> Create(Create.Query query)
        {
            var model = await _mediator.Send(query);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Create.Command command)
        {
            await _mediator.Send(command);
            // return this.RedirectToActionJson(nameof(Index));
            return this.RedirectToActionJson(nameof(Detail), new { ID = command.FormID, Status = "New" });

        }

        public async Task<IActionResult> CreateStateProgram(CreateStateProgram.Query query)
        {
            var model = await _mediator.Send(query);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStateProgram(CreateStateProgram.Command command)
        {
            await _mediator.Send(command);
            return this.RedirectToActionJson(nameof(Detail), new { ID = command.FormID });
        }

        public async Task<IActionResult> Edit(Edit.Query query)
        {
            var model = await _mediator.Send(query);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Edit.Command command)
        {
            await _mediator.Send(command);
            return this.RedirectToActionJson(nameof(Detail), new { ID = command.FormID });
        }

        public async Task<IActionResult> EditStateProgram(EditStateProgram.Query query)
        {
            var model = await _mediator.Send(query);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStateProgram(EditStateProgram.Command command)
        {
            await _mediator.Send(command);
            return this.RedirectToActionJson(nameof(Detail), new { ID = command.FormID });
        }

        public async Task<IActionResult> Delete(Delete.Query query)
        {
            var model = await _mediator.Send(query);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UnitOfWork]
        public async Task<IActionResult> Delete(Delete.Command command)
        {
            await _mediator.Send(command);
            return this.RedirectToActionJson(nameof(Index));
        }

        public async Task<IActionResult> DeleteStateProgram(DeleteStateProgram.Query query)
        {
            var model = await _mediator.Send(query);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStateProgram(DeleteStateProgram.Command command)
        {
            await _mediator.Send(command);
            return this.RedirectToActionJson(nameof(Detail), new { ID = command.FormID });
        }

        public async Task<IActionResult> Export(Export.Query query)
        {
            var result = await _mediator.Send(query);
            return File(result.FileContents, result.ContentType, result.FileName);
        }
        public async Task<IActionResult> GetPDF(GetPDF.Query query)
        {
            var model = await _mediator.Send(query);
            return File(model.FileContents, model.ContentType);
        }
    }
}