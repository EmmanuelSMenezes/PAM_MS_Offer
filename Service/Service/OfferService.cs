using Domain.Model;
using Infrastructure.Repository;
using Microsoft.CodeAnalysis.Operations;
using Serilog;
using System;

namespace Application.Service
{
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _repository;
        private readonly ILogger _logger;
        private readonly string _privateSecretKey;
        private readonly string _tokenValidationMinutes;

        public OfferService(IOfferRepository repository, ILogger logger, string privateSecretKey, string tokenValidationMinutes)
        {
            _repository = repository;
            _logger = logger;
            _privateSecretKey = privateSecretKey;
            _tokenValidationMinutes = tokenValidationMinutes;
        }

    public ProductsBranchOffer GetProductOffersByLocationPoint(Guid branch_id, FilterOffer filter)
    {
            try
            {
                return _repository.GetProductOffersByLocationPoint(branch_id, filter);
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
      
    }

        public Offer GetBranchOffersByLocationPoint(string latitude, string longitude, FilterOffer filter)
        {
            try
            {
                return _repository.GetBranchOffersByLocationPoint(latitude, longitude, filter);
            }
            catch (System.Exception ex)
            {

                throw ex;
            }

        }

        public OfferProducts GetProductOffersByLocationPoint(string latitude, string longitude, FilterOffer filter)
        {
            try
            {
                return _repository.GetProductOffersByLocationPoint(latitude, longitude, filter);
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        public bool GetBranchByLocationPoint(string latitude, string longitude, Guid branch_id)
        {
            try
            {
                return _repository.GetBranchByLocationPoint(latitude, longitude, branch_id);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public OfferFilters GetFiltersByLocationPoint(string latitude, string longitude, string filter)
        {
            try
            {
                return _repository.GetFiltersByLocationPoint(latitude, longitude, filter);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
