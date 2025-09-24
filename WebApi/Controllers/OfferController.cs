using System;
using System.ComponentModel.DataAnnotations;
using Application.Service;
using Domain.Model;
using Domain.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace WebApi.Controllers
{
    [Route("offer")]
    [ApiController]
    public class OfferController : Controller
    {
        private readonly IOfferService _service;
        private readonly ILogger _logger;

        public OfferController(IOfferService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint responsável por buscar produto pelo id.
        /// </summary>
        /// <returns>Retorna produtos cadastrados.</returns>
        [Authorize]
        [HttpGet("productOffersByBranch")]
        [ProducesResponseType(typeof(Response<ProductsBranchOffer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ProductsBranchOffer>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ProductsBranchOffer>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<ProductsBranchOffer>> GetProductOffersByBranch([FromQuery][Required] Guid branch_id, string filter, int? page, int? itensPerPage)
        {
            try
            {

                var filters = new FilterOffer
                {
                    Page = page ?? 1,
                    ItensPerPage = itensPerPage ?? 5,
                    Filter = filter
                };

                var response = _service.GetProductOffersByLocationPoint(branch_id, filters);
                return StatusCode(StatusCodes.Status200OK, new Response<ProductsBranchOffer>() { Status = 200, Message = $"Produto retornado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing product!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ProductsBranchOffer>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por buscar unidade pela localidade.
        /// </summary>
        /// <returns>Retorna unidades cadastrados.</returns>
        [Authorize]
        [HttpGet("branchOffersByLocationPoint")]
        [ProducesResponseType(typeof(Response<Offer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<Offer>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<Offer>), StatusCodes.Status500InternalServerError)]

        public ActionResult<Response<Offer>> GetBranchOffersByLocationPoint([FromQuery][Required] string latitude, [Required] string longitude, string filter, int? page, int? itensPerPage, [FromQuery] OrderByBranch orderBy, [FromQuery] SortOrder sort)
        {
            try
            {

                var filters = new FilterOffer
                {
                    Page = page ?? 1,
                    ItensPerPage = itensPerPage ?? 5,
                    Filter = filter,
                    SortOrder = sort,
                    OrderByBranch = orderBy,
                };

                var response = _service.GetBranchOffersByLocationPoint(latitude, longitude, filters);
                return StatusCode(StatusCodes.Status200OK, new Response<Offer>() { Status = 200, Message = $"Unidades retornadas com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing product!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<Offer>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por buscar produtos pela localidade.
        /// </summary>
        /// <returns>Retorna produtos cadastrados.</returns>
        [Authorize]
        [HttpGet("productOffersByLocationPoint")]
        [ProducesResponseType(typeof(Response<OfferProducts>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<OfferProducts>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<OfferProducts>), StatusCodes.Status500InternalServerError)]

        public ActionResult<Response<OfferProducts>> GetProductsOffersByLocationPoint([FromQuery][Required] string latitude, [Required] string longitude, string filter, string category_ids, string branch_ids, string delivery_option_ids, string ratings, string? distance, string? start_price, string? end_price, bool? shipping_free, int? page, int? itensPerPage, [FromQuery] SortOrder sort_price, [FromQuery] OrderByProduct? orderBy)
        {
            try
            {

                var filters = new FilterOffer
                {
                    Page = page ?? 1,
                    ItensPerPage = itensPerPage ?? 5,
                    Filter = filter,
                    Category_ids = category_ids,
                    Branch_ids = branch_ids,
                    Delivery_option_ids = delivery_option_ids,
                    Ratings = ratings, 
                    Distance = distance ?? "infinity",
                    Start_price = start_price ?? "-infinity",
                    End_price = end_price ?? "infinity",
                    SortOrder = sort_price,
                    Shipping_free = shipping_free ?? false,
                    OrderByProduct = orderBy?? OrderByProduct.Ratings
                };

                var response = _service.GetProductOffersByLocationPoint(latitude, longitude, filters);
                return StatusCode(StatusCodes.Status200OK, new Response<OfferProducts>() { Status = 200, Message = $"Produtos retornado com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing product!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<OfferProducts>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por verificar se filial atende ao endereço do consumidor.
        /// </summary>
        /// <returns>Retorna verdairo caso filial atenda e falso caso não atenda.</returns>
        [Authorize]
        [HttpGet("branchByLocationPoint")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status500InternalServerError)]

        public ActionResult<Response<bool>> GetBranchByLocationPoint([FromQuery][Required] string latitude, [Required] string longitude, [Required] Guid branch_id)
        {
            try
            {

                var response = _service.GetBranchByLocationPoint(latitude, longitude, branch_id);
                var mensage = response ? "Filial atende ao endereço informado!" : "Filial não atende ao endereço informado!";
                return StatusCode(StatusCodes.Status200OK, new Response<bool>() { Status = 200, Message = $"{mensage}", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception when checking branch!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<bool>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por buscar filtros do produto para o consumidor.
        /// </summary>
        /// <returns>Retorna filtros ao consumidor.</returns>
        [Authorize]
        [HttpGet("filtersByLocationPoint")]
        [ProducesResponseType(typeof(Response<OfferFilters>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<OfferFilters>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<OfferFilters>), StatusCodes.Status500InternalServerError)]

        public ActionResult<Response<OfferFilters>> GetFiltersByLocationPoint([FromQuery][Required] string latitude, [Required] string longitude, string filter)
        {
            try
            {

                var response = _service.GetFiltersByLocationPoint(latitude, longitude, filter);
                return StatusCode(StatusCodes.Status200OK, new Response<OfferFilters>() { Status = 200, Message = $"Filtros retornado com sucesso!", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception when listing filters!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<OfferFilters>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false, Error = ex });
                }
            }
        }
    }
}
