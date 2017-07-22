﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public class PeopleController : FacesBaseController
    {
        public async Task<ActionResult> Index(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return HttpNotFound("Person group ID is missing");
            }

            using (var client = GetFaceClient())
            {
                var model = await client.ListPersonsAsync(id);
                ViewBag.PersonGroupId = id;

                return View(model);
            }
        }

        public async Task<ActionResult> Details(string id, Guid? personId)
        {
            if(string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            if(personId == null)
            {
                return HttpNotFound();
            }

            using (var client = GetFaceClient())
            {
                var model = await client.GetPersonAsync(id, personId.Value);
                ViewBag.PersonGroupId = id;

                return View(model);
            }
        }

        public ActionResult Create(string id)
        {
            ViewBag.PersonGroupId = id;

            return View("Edit", new Person());
        }

        [HttpPost]
        public async Task<ActionResult> Create(Person person)
        {
            return await Edit(person);
        }

        public async Task<ActionResult> Edit(string id, Guid personId)
        {
            ViewBag.PersonGroupId = id;

            using (var client = GetFaceClient())
            {
                var model = await client.GetPersonAsync(id, personId);

                return View(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Person person)
        {
            var personGroupId = Request.Form["PersonGroupId"];
            if(string.IsNullOrEmpty(personGroupId))
            {
                return HttpNotFound("PersonGroupId is missing");
            }

            if(!ModelState.IsValid)
            {
                ViewBag.PersonGroupId = personGroupId;
                return View(person);
            }

            try
            {
                using (var client = GetFaceClient())
                {
                    if(person.PersonId == Guid.Empty)
                    {
                        await client.CreatePersonAsync(personGroupId, person.Name, person.UserData);
                    }
                    else
                    {
                        await client.UpdatePersonAsync(personGroupId, person.PersonId, person.Name, person.UserData);
                    }

                    return RedirectToAction("Index", new { id = personGroupId });
                }
            }
            catch (FaceAPIException fex)
            {
                ModelState.AddModelError(string.Empty, fex.ErrorMessage);
            }

            return View(person);
        }

        [HttpGet]
        public ActionResult AddFace(string id, string personId)
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddFace()
        {
            var id = Request["id"];
            var personId = Guid.Parse(Request["personId"]);

            using (var client = GetFaceClient())
            {
                try
                {
                    await client.AddPersonFaceAsync(id, personId, Request.Files[0].InputStream);
                }
                catch(Exception ex)
                {
                    ViewBag.Error = ex.Message;
                    return View();
                }
            }

            return RedirectToAction("Index", new { id = id });
        }
    }
}