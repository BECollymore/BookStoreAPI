using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class BooksController : Controller
    {

        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;


        public BooksController(IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all Books
        /// </summary>
        /// <returns>list of all books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            string location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo("Sucesfully got all Books");
                return Ok(response);

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
        /// Gets an book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A book's record</returns>
        // GET api/values/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            string location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to get an Book by Id:{id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"{location}: Author with Id:{id} was not found");
                    NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                return Ok(response);

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");

            }

        }

        /// <summary>
        /// Creates a book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns>book object</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to create book");
                if(bookDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty request was submitted");
                    return BadRequest(ModelState);
                }
                if(!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Empty request was submitted");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var IsSuccesful = await _bookRepository.Create(book);
                if(!IsSuccesful)
                {
                    return InternalError($"{location}: creation failed");
                }
                _logger.LogInfo($"{location}: Book created sucessfully");
                return Created("create", new {book});

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }

        /// <summary>
        /// Updates a book by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookupdateDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookupdateDTO)
        {

            string location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempting to update book");
                if (id < 1 || bookupdateDTO == null || id != bookupdateDTO.Id)
                {
                    _logger.LogWarn("Book must have required fields and not empty");
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    InternalError($"{location}: Data was not complete");
                    return BadRequest(ModelState);

                }
                var isexist = await _bookRepository.isExists(id);
                if (!isexist)
                {
                    _logger.LogInfo($"{location}: Book was not found in Database");
                    return NotFound();
                }

                var book = _mapper.Map<Book>(bookupdateDTO);
                var IsSuccessful = await _bookRepository.Update(book);
                if (!IsSuccessful)
                {
                    return InternalError($"{location}: Error updating book data");
                }
                _logger.LogInfo($"{location}: book with id:{book.Id} was created successfully");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Delet a book
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            string location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempting call to delete book");
                if (id < 1)
                {
                    return BadRequest();
                }
                //see if book exists
                var isexist = await _bookRepository.isExists(id);
                if(!isexist)
                {
                    _logger.LogInfo($"{location}: Book was not found in database");
                    return NotFound();
                }
                var author = await _bookRepository.FindById(id);
                var IsSuccessful = await _bookRepository.Delete(author);
                if (!IsSuccessful)
                {
                    _logger.LogInfo($"{location}: attempt to delete book failed");
                    return InternalError($"{location}: Error deleting book");

                }
                _logger.LogInfo($"{location}: Successfully deleted book");
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");

            }
        }


    }
}
