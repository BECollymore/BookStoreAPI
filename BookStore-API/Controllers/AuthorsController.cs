using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using BookStore_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookStore_API.Controllers
{
    /// <summary>
    /// endpoint used to interact with authors in the bookstore database
    /// </summary>
    [Route("api/[controller]")]
    [Authorize] //global authorize, so only authorized users can use API
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;


        public AuthorsController(IAuthorRepository authorRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all authors
        /// </summary>
        /// <returns>list of all authors</returns>
        [HttpGet]
        [Authorize(Roles = "Administrator, Customer")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            string location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting Call");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo($"{location}: Succesfull");
                return Ok(response);

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }
        /// <summary>
        /// Gets an author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An author's record</returns>
        // GET api/values/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            string location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to get an author by Id:{id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"{location}: Author with Id:{id} was not found");
                    NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                return Ok(response);

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");

            }

        }

        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles ="Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            string location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to create author");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"{location}: Author must have required fields and not empty");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    InternalError($"{location}: Data was not complete");
                    return BadRequest(ModelState);

                }

                var author = _mapper.Map<Author>(authorDTO);
                var IsSuccessful = await _authorRepository.Create(author);
                if (!IsSuccessful)
                {
                    return InternalError($"{location}: Error creating author");
                }

                _logger.LogInfo($"{location}: Author created successfully");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");

            }

        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            // i.e internal server error message
            return StatusCode(500, "Something went wrong. Please contact API administrator.");
        }

        /// <summary>
        /// Updates an Author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorupdateDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id,[FromBody] AuthorUpdateDTO authorupdateDTO)
        {
           string location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to update author");
                if (id < 1 || authorupdateDTO == null || id != authorupdateDTO.Id)
                {
                    _logger.LogWarn("Author must have required fields and not empty");
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    InternalError($"{location}: Data was not complete");
                    return BadRequest(ModelState);

                }
                var isexist = await _authorRepository.isExists(id);
                if(!isexist)
                {
                    _logger.LogInfo($"{location}: Author was not found in Database");
                    return NotFound();
                }

                var author = _mapper.Map<Author>(authorupdateDTO);
                var IsSuccessful = await _authorRepository.Update(author);
                if(!IsSuccessful)
                {
                    return InternalError($"{location}: Error updating author data");
                }
                _logger.LogInfo($"{location}: Author with id:{author.Id} was created successfully");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }

        /// <summary>
        /// Deletes an author
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE api/values/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            string location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting call to delete author");
                if (id < 1)
                {
                    return BadRequest();
                }
                var isexist = await _authorRepository.isExists(id);
                if (!isexist)
                {
                    _logger.LogInfo($"{location}: Author was not found in Database");
                    return NotFound();
                }

                var author = await _authorRepository.FindById(id);
                var IsSuccessful = await _authorRepository.Delete(author);
                if(!IsSuccessful)
                {
                    _logger.LogInfo($"{location}: attempt to delete author failed");
                    return InternalError($"{location}: Error deleting author");

                }
                _logger.LogInfo($"{location}: Successfully deleted author");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
    }
}
