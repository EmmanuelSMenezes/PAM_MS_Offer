using Domain.Model;
using Microsoft.CodeAnalysis.Operations;
using System;

namespace Application.Service
{
    public interface IOfferService
    {
        ProductsBranchOffer GetProductOffersByLocationPoint(Guid branch_id, FilterOffer filter);
        Offer GetBranchOffersByLocationPoint(string latitude, string longitude, FilterOffer filter);
        OfferProducts GetProductOffersByLocationPoint(string latitude, string longitude, FilterOffer filter);
        bool GetBranchByLocationPoint(string latitude, string longitude, Guid branch_id);
        OfferFilters GetFiltersByLocationPoint(string latitude, string longitude, string filter);
    }
}
