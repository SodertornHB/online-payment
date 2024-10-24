
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//--------------------------------------------------------------------------------------------------------------------

using AutoMapper;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlinePayment.Web.Controllers
{
    public partial class AuditController : Controller
    {
        private readonly ILogger<AuditController> logger;
        private readonly IAuditService service;
        private readonly IMapper mapper;

        public AuditController(ILogger<AuditController> logger, 
        IAuditService service, 
        IMapper mapper)
        {
            this.logger = logger;
            this.service = service;
            this.mapper = mapper;
        }

        public virtual async Task<IActionResult> Index()
        {
            var list = await service.GetAll();
            var viewModels = mapper.Map<IEnumerable<AuditViewModel>>(list);
            return View(viewModels.OrderByDescending(x => x.Id));
        }
        
        public ActionResult Create()
        {
            return View(new AuditViewModel());
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create([FromForm]AuditViewModel viewModel)
        {
            var model = mapper.Map<Audit>(viewModel);
            await service.Insert(model);
            return RedirectToAction(nameof(Index));
        }

        public virtual async Task<ActionResult> Edit(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<AuditViewModel>(entity));
        }


        [HttpPost]
        public virtual async Task<ActionResult> Edit([FromForm]AuditViewModel viewModel)
        {
            var model = mapper.Map<Audit>(viewModel);
            await service.Update(model);
            return RedirectToAction(nameof(Index));         
        }

        public virtual async Task<ActionResult> Remove(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<AuditViewModel>(entity));        
        }

        [HttpPost]
        public virtual async Task<ActionResult> Remove([FromForm]AuditViewModel viewModel)
        {
            var model = mapper.Map<Audit>(viewModel);
            await service.Delete(viewModel.Id);
            return RedirectToAction(nameof(Index));         
        }
    }
}