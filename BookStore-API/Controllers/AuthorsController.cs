using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using BookStore_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookStore_API.Controllers
{
    /// <summary>
    /// endpoint used to interact with authors in the bookstore database
    /// </summary>
    [Route("api/[controller]")]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempting Get All Authors request");
                var authors =  await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Sucesfully got all Authors");
                return Ok(response);
                
            }
            catch(Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
           
        }
        /// <summary>
        /// Gets an author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An author's record</returns>
        // GET api/values/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempting to get an author by Id:{id}");
                var author = await _authorRepository.FindById(id);
                if(author == null)
                {
                    _logger.LogWarn($"Author with Id:{id} was not found");
                    NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                return Ok(response);

            }
            catch (Exception e)
            {
               return InternalError($"{e.Message} - {e.InnerException}");
               
            }
           
        }
        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>

        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo("Attempting to create author");
                if (authorDTO == null)
                {
                    _logger.LogWarn("Author must have required fields and not empty");
                    return BadRequest(ModelState);
                }
                if(!ModelState.IsValid)
                {
                    InternalError("Data was not complete");
                    return BadRequest(ModelState);

                }
                
                var author = _mapper.Map<Author>(authorDTO);
                var IsSuccessful = await _authorRepository.Create(author);
                if(!IsSuccessful)
                {
                   return InternalError("Error creating author");
                }

                _logger.LogInfo("Author created successfully");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");

            }

        }
        
        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            // i.e internal server error message
            return StatusCode(500, "Something went wrong. Please contact API administrator.");
        }


        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
