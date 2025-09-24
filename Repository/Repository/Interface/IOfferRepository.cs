using Domain.Model;
using System;

namespace Infrastructure.Repository
{
  public interface IOfferRepository
    {
        ProductsBranchOffer GetProductOffersByLocationPoint(Guid branch_id, FilterOffer filter);
        Offer GetBranchOffersByLocationPoint(string latitude, string longitude, FilterOffer filter);
        OfferProducts GetProductOffersByLocationPoint(string latitude, string longitude, FilterOffer filter);
        bool GetBranchByLocationPoint(string latitude, string longitude, Guid branch_id);
        OfferFilters GetFiltersByLocationPoint(string latitude, string longitude, string filter);
    }
}
