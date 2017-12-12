﻿using System;
using System.Net;
using System.Threading.Tasks;
using ImageServer.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ImageServer.Core.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileAccessService _file;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileAccessService fileService, ILogger<FileController> logger)
        {
            _file = fileService;
            _logger = logger;
        }

        [HttpGet("/f/{host}/{id:gridfs}.{ext}")]
        [HttpGet("/f/{host}/{*id}")]
        public async Task<IActionResult> FileAsync(string host, string id)
        {
            byte[] bytes;
            try
            {
                bytes = await _file.GetFileAsync(host, id);
            }
            catch (SlugNotFoundException e)
            {
                _logger.LogError(e.Message);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            catch (GridFsObjectIdException e)
            {
                _logger.LogError("GridFS ObjectId Parse Error:" + e.Message);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            if (bytes == null)
            {
                _logger.LogError("File not found");
                return NotFound();
            }

            return File(bytes, "application/octet-stream");
        }
    }
}
